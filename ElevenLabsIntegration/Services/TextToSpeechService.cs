using ElevenLabsIntegration.Console.Constants;
using ElevenLabsIntegration.Console.Models;

namespace ElevenLabsIntegration.Console.Services;

public class TextToSpeechService
{
    private readonly ElevenLabsClient _client;
    private readonly ILogger _logger;
    private readonly IAudioFileService _audioFileService;

    public TextToSpeechService(
        ElevenLabsClient client, 
        ILogger logger,
        IAudioFileService audioFileService)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioFileService = audioFileService ?? throw new ArgumentNullException(nameof(audioFileService));
    }

    public async Task<string> SpeakTextAsync(string voiceId, string text, CancellationToken cancellationToken = default)
    {
        var outputFilePath = string.Empty;
        
        try
        {
            // Generate a unique file path for this audio
            outputFilePath = GetAudioFilePath();
            _logger.Log($"Audio will be saved to: {outputFilePath}");
            
            // Connect to the WebSocket
            await _client.ConnectAsync(voiceId, cancellationToken);

            // Initialize the text-to-speech session
            var initRequest = new TextToSpeechInitRequest
            {
                Text = " ",
                VoiceSettings = new VoiceSettings
                {
                    Stability = 0.5,
                    SimilarityBoost = 0.8,
                    Speed = 1.0
                }
            };

            await _client.InitializeTextToSpeechAsync(initRequest, cancellationToken);

            // Send the actual text
            await _client.SendTextChunkAsync(text, true, cancellationToken);

            // Signal end of text
            await _client.FinalizeTextAsync(cancellationToken);

            // Flag to determine if this is the first chunk we're processing
            bool isFirstChunk = true;

            // Receive and process audio chunks
            while (_client != null)
            {
                var response = await _client.ReceiveAudioChunkAsync(cancellationToken);
                
                if (response.HasAudioData)
                {
                    _logger.Log($"Received audio chunk with {response.Audio.Length} bytes");
                    
                    if (response.Alignment != null)
                    {
                        _logger.Log($"Audio alignment: {response.Alignment.Chars?.Length ?? 0} characters");
                    }
                    
                    // Save or append the audio chunk
                    if (isFirstChunk)
                    {
                        await _audioFileService.SaveAudioChunkAsync(response.Audio, outputFilePath, cancellationToken);
                        isFirstChunk = false;
                    }
                    else
                    {
                        await _audioFileService.AppendAudioChunkAsync(response.Audio, outputFilePath, cancellationToken);
                    }
                }
                else
                {
                    // Check if this is the final message
                    _logger.Log("Received a response without audio data");
                    
                    if (response.IsLastChunk)
                    {
                        _logger.Log("Received final message from server");
                        await _audioFileService.FinalizeAudioFileAsync(outputFilePath, cancellationToken);
                        break;
                    }
                }
            }
            
            return outputFilePath;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error during speech generation: {ex.Message}");
            throw;
        }
        finally
        {
            // Ensure we close the connection
            await _client.DisconnectAsync(cancellationToken);
        }
    }
    
    private string GetAudioFilePath()
    {
        // Use the exact path specified in FileConstants
        return _audioFileService.GenerateUniqueAudioFilePath(
            FileConstants.AudioFilesDirectory,
            FileConstants.DefaultAudioPrefix,
            FileConstants.Mp3Extension);
    }
}
