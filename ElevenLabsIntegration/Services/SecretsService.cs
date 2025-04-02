using ElevenLabsIntegration.Console.Configurations;
using ElevenLabsIntegration.Console.Constants;

namespace ElevenLabsIntegration.Console.Services;

public class SecretsService(IEnvironmentConfiguration envConfig)
{
    public string GetApiKey()
    {
        envConfig.Reload();

        var apiKey = Environment.GetEnvironmentVariable(SecretConstants.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API_KEY not found in environment variables");
        }
        
        return apiKey;
    }
}
