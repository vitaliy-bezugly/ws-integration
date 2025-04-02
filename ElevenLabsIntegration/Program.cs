using ElevenLabsIntegration.Console;
using ElevenLabsIntegration.Console.Configurations;
using ElevenLabsIntegration.Console.Constants;
using ElevenLabsIntegration.Console.Services;

var envConfig = new EnvironmentConfiguration();
var secretsService = new SecretsService(envConfig);
var apiKey = secretsService.GetApiKey();
var logger = new ConsoleLogger();
var audioFileService = new AudioFileService(logger);

logger.Log($"API is ready to use with ElevenLabs at {ApiConstants.Domain}");
logger.Log($"API Key length: {apiKey.Length}");

// Verify output directory is accessible
try
{
    var directoryInfo = new DirectoryInfo(FileConstants.AudioFilesDirectory);
    
    if (!directoryInfo.Exists)
    {
        directoryInfo.Create();
        logger.Log($"Created output directory: {FileConstants.AudioFilesDirectory}");
    }
    
    // Test write access by creating and deleting a temporary file
    var testFilePath = Path.Combine(FileConstants.AudioFilesDirectory, $"test_{Guid.NewGuid()}.tmp");
    File.WriteAllText(testFilePath, "Test");
    File.Delete(testFilePath);
    logger.Log($"Confirmed write access to: {FileConstants.AudioFilesDirectory}");
}
catch (Exception ex)
{
    logger.Log($"ERROR: Cannot access output directory: {ex.Message}");
    logger.Log("Please run the application with appropriate permissions or modify FileConstants.AudioFilesDirectory path");
    return; // Exit if we can't access the directory
}

using (var client = new ElevenLabsClient(apiKey, logger))
{
    var ttsService = new TextToSpeechService(client, logger, audioFileService);
    
    logger.Log("Starting text-to-speech demo...");
    
    try
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMinutes(2)); 
        
        string textToSpeak = "Hello world! This is a test of the ElevenLabs WebSocket API.";
        
        logger.Log($"Converting text to speech: \"{textToSpeak}\"");
        string outputFilePath = await ttsService.SpeakTextAsync(ApiConstants.VoiceId, textToSpeak, cts.Token);
        
        logger.Log($"Text-to-speech completed successfully. Audio saved to: {outputFilePath}");
    }
    catch (Exception ex)
    {
        logger.Log($"Error during text-to-speech demo: {ex.Message}");
    }
}

logger.Log("Demo completed.");
