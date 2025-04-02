using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ElevenLabsIntegration.Console.Constants;
using ElevenLabsIntegration.Console.Models;
using ElevenLabsIntegration.Console.Services;

namespace ElevenLabsIntegration.Console;

public class ElevenLabsClient : IDisposable
{
    private readonly ClientWebSocket _clientWebSocket;
    private readonly ILogger _logger;
    private bool _isConnected = false;

    public ElevenLabsClient(string apiKey, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey, nameof(apiKey));
        
        _clientWebSocket = new ClientWebSocket();
        _clientWebSocket.Options.SetRequestHeader("xi-api-key", apiKey);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ConnectAsync(string voiceId, CancellationToken cancellationToken)
    {
        if (_isConnected)
        {
            _logger.Log("Already connected to ElevenLabs API");
            return;
        }

        var endpoint = string.Format(ApiConstants.TextToSpeechStreamEndpoint, voiceId);
        var uri = new Uri($"wss://{ApiConstants.Domain}/{endpoint}");
        
        await _clientWebSocket.ConnectAsync(uri, cancellationToken);
        _isConnected = true;
    }

    public async Task InitializeTextToSpeechAsync(TextToSpeechInitRequest initRequest, CancellationToken cancellationToken)
    {
        EnsureConnected();
        
        var json = JsonSerializer.Serialize(initRequest);
        _logger.Log($"Initializing Text-to-Speech with settings: {json}");
        await SendMessageAsync(json, cancellationToken);
    }

    public async Task SendTextChunkAsync(string text, bool triggerGeneration = true, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        
        var request = new TextChunkRequest
        {
            Text = text,
            TryTriggerGeneration = triggerGeneration
        };
        
        var json = JsonSerializer.Serialize(request);
        _logger.Log($"Sending text chunk: {json}");
        await SendMessageAsync(json, cancellationToken);
    }
    
    public async Task FinalizeTextAsync(CancellationToken cancellationToken)
    {
        EnsureConnected();
        
        var request = new TextChunkRequest { Text = "" };
        var json = JsonSerializer.Serialize(request);
        
        _logger.Log("Finalizing text stream");
        await SendMessageAsync(json, cancellationToken);
    }

    private async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var messageBuffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBuffer);
        await _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task<AudioChunkResponse> ReceiveAudioChunkAsync(CancellationToken cancellationToken)
    {
        EnsureConnected();
        
        var buffer = new byte[8192]; // Larger buffer for audio data
        var segment = new ArraySegment<byte>(buffer);
        
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;
        
        do
        {
            result = await _clientWebSocket.ReceiveAsync(segment, cancellationToken);
            memoryStream.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        
        if (result.MessageType == WebSocketMessageType.Text)
        {
            using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
            var jsonResponse = await streamReader.ReadToEndAsync(cancellationToken);
            _logger.Log($"Received response: {jsonResponse}");
            
            return JsonSerializer.Deserialize<AudioChunkResponse>(jsonResponse) ?? 
                   new AudioChunkResponse();
        }
        
        _logger.Log("Received non-text message type");
        return new AudioChunkResponse();
    }

    private void EnsureConnected()
    {
        if (!_isConnected || _clientWebSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected. Call ConnectAsync first.");
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_isConnected && _clientWebSocket.State == WebSocketState.Open)
        {
            _logger.Log("Closing WebSocket connection");
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            _isConnected = false;
        }
    }

    public void Dispose()
    {
        _clientWebSocket.Dispose();
        GC.SuppressFinalize(this);
    }
}
