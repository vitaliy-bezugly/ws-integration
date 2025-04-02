using System.Text.Json.Serialization;

namespace ElevenLabsIntegration.Console.Models;

public class TextToSpeechInitRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("voice_settings")]
    public VoiceSettings VoiceSettings { get; set; } = new();
}

public class VoiceSettings
{
    [JsonPropertyName("stability")]
    public double Stability { get; set; } = 0.5;
    
    [JsonPropertyName("similarity_boost")]
    public double SimilarityBoost { get; set; } = 0.8;
    
    [JsonPropertyName("speed")]
    public double Speed { get; set; } = 1.0;
}

public class TextChunkRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("try_trigger_generation")]
    public bool TryTriggerGeneration { get; set; }
}

public class AudioChunkResponse
{
    [JsonPropertyName("audio")]
    public string Audio { get; set; } = string.Empty;
    
    [JsonPropertyName("normalizedAlignment")]
    public AlignmentInfo? NormalizedAlignment { get; set; }
    
    [JsonPropertyName("alignment")]
    public AlignmentInfo? Alignment { get; set; }
    
    [JsonPropertyName("isFinal")]
    public bool? IsFinal { get; set; }
    
    public bool HasAudioData => !string.IsNullOrEmpty(Audio);
    
    public bool IsLastChunk => IsFinal.HasValue && IsFinal.Value;
}

public class AlignmentInfo
{
    [JsonPropertyName("char_start_times_ms")]
    public int[]? CharStartTimesMsec { get; set; }
    
    [JsonPropertyName("chars_durations_ms")]
    public int[]? CharsDurationsMsec { get; set; }
    
    [JsonPropertyName("chars")]
    public string[]? Chars { get; set; }
}
