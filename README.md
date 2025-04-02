A .NET 9.0 console application demonstrating how to integrate with the ElevenLabs Text-to-Speech API using WebSockets. This project serves as an educational example for implementing client-side WebSocket communication with ElevenLabs.

## Project Overview

This application showcases:
- Establishing WebSocket connections to ElevenLabs API
- Sending text to be converted to speech
- Receiving audio data via WebSocket streaming
- Saving the received audio to local files

## Features

- **WebSocket Communication**: Direct streaming connection with ElevenLabs API
- **Text-to-Speech Conversion**: Convert text strings to natural-sounding speech
- **Environment Configuration**: Secure API key management via .env files
- **Structured Error Handling**: Comprehensive error checking and reporting
- **File Management**: Audio output file handling with permission verification

## Project Structure

```
ElevenLabsIntegration/
├── Configurations/           # Environment and configuration setup
├── Constants/                # API endpoints and file path constants
├── Files/                    # Output directory for generated audio files
├── Models/                   # Data models for API requests/responses
└── Services/                 # Core functionality implementation
    ├── TextToSpeechService   # High-level TTS service
    ├── AudioFileService      # File handling operations
    └── SecretsService        # API key management
```

## How It Works

1. The application loads configuration including the API key from environment variables
2. It establishes a WebSocket connection to ElevenLabs API
3. Text is sent through the WebSocket connection
4. Audio data is received as chunks and assembled into an audio file
5. The resulting MP3 file is saved locally

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- ElevenLabs API key

### Setup

1. Clone the repository
2. Create a `.env` file in the project root with your API key:
   ```
   ELEVENLABS_API_KEY=your_api_key_here
   ```
3. Build the project:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run
   ```

## Example Usage

The application demonstrates a simple text-to-speech conversion:

```csharp
// Create client and services
using (var client = new ElevenLabsClient(apiKey, logger))
{
    var ttsService = new TextToSpeechService(client, logger, audioFileService);
    
    // Convert text to speech
    string textToSpeak = "Hello world! This is a test of the ElevenLabs WebSocket API.";
    string outputFilePath = await ttsService.SpeakTextAsync(ApiConstants.VoiceId, textToSpeak);
}
```

## Disclaimer

This is a demonstration project and not intended for production use. It is designed as an educational resource for understanding WebSocket-based API integration.

## Dependencies

- [DotNetEnv](https://www.nuget.org/packages/DotNetEnv/) - For environment variable management