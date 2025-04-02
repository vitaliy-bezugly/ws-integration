namespace ElevenLabsIntegration.Console.Services;

public interface IAudioFileService
{
    Task SaveAudioChunkAsync(string base64Audio, string outputPath, CancellationToken cancellationToken = default);
    Task AppendAudioChunkAsync(string base64Audio, string outputPath, CancellationToken cancellationToken = default);
    Task FinalizeAudioFileAsync(string outputPath, CancellationToken cancellationToken = default);
    string GenerateUniqueAudioFilePath(string directory, string prefix = "speech", string extension = ".mp3");
}
