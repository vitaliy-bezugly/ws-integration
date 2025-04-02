namespace ElevenLabsIntegration.Console.Services;

public class AudioFileService : IAudioFileService
{
    private readonly ILogger _logger;

    public AudioFileService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveAudioChunkAsync(string base64Audio, string outputPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(base64Audio))
        {
            _logger.Log("Empty audio chunk received, skipping save operation");
            return;
        }

        try
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.Log($"Created directory: {directory}");
            }

            // Convert base64 to bytes and save
            var audioBytes = Convert.FromBase64String(base64Audio);
            await File.WriteAllBytesAsync(outputPath, audioBytes, cancellationToken);
            _logger.Log($"Saved audio file to: {outputPath} ({audioBytes.Length} bytes)");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error saving audio file: {ex.Message}");
            throw;
        }
    }

    public async Task AppendAudioChunkAsync(string base64Audio, string outputPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(base64Audio))
        {
            _logger.Log("Empty audio chunk received, skipping append operation");
            return;
        }

        try
        {
            var audioBytes = Convert.FromBase64String(base64Audio);
            using (var fileStream = new FileStream(outputPath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                await fileStream.WriteAsync(audioBytes, cancellationToken);
            }
            _logger.Log($"Appended {audioBytes.Length} bytes to: {outputPath}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error appending to audio file: {ex.Message}");
            throw;
        }
    }

    public Task FinalizeAudioFileAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.Log($"Audio file finalized: {outputPath}");
        return Task.CompletedTask;
    }

    public string GenerateUniqueAudioFilePath(string directory, string prefix = "speech", string extension = ".mp3")
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(directory))
            {
                _logger.Log($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
                _logger.Log($"Successfully created directory: {directory}");
            }
            else
            {
                _logger.Log($"Directory already exists: {directory}");
            }

            // Generate a unique filename with timestamp and random GUID suffix
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var filename = $"{prefix}_{timestamp}_{uniqueId}{extension}";
            
            var fullPath = Path.Combine(directory, filename);
            _logger.Log($"Generated file path: {fullPath}");
            
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error generating file path: {ex.Message}");
            // Fallback to temp directory if there are permission issues
            var tempPath = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid():N}{extension}");
            _logger.Log($"Using fallback temp path: {tempPath}");
            return tempPath;
        }
    }
}
