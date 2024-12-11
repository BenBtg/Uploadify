# VoiceVault.AudioPlugin

VoiceVault.AudioPlugin is a work-in-progress (WIP) audio recorder based on the MauiAudio Plugin. It is designed to demonstrate background audio recording capabilities using .NET MAUI. This project is not required to test the VoiceVault.Maui uploader app.

## Project Structure

The project is organized into the following directories:

- `Converters/`: Contains value converters used in the application.
- `Pages/`: Contains the XAML pages for the application.
- `Platforms/`: Contains platform-specific code for Android, iOS, MacCatalyst, Tizen, and Windows.
- `Resources/`: Contains application resources such as images, fonts, and raw assets.
- `ViewModels/`: Contains the view models for the application.

## Features

- Background audio recording using the MauiAudio Plugin.
- Demonstrates the use of various audio-related functionalities such as playback, pause, stop, and seek.
- Supports multiple platforms including Android, iOS, MacCatalyst, and Windows.

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or later with MAUI workload installed

### Building the Project

1. Clone the repository:
    ```sh
    git clone https://github.com/your-repo/VoiceVault.AudioPlugin.git
    cd VoiceVault.AudioPlugin
    ```

2. Open the solution file `VoiceVaultAPI.sln` in Visual Studio.

3. Restore the NuGet packages:
    ```sh
    dotnet restore
    ```

4. Build the solution:
    ```sh
    dotnet build
    ```

### Running the Project

1. Set `VoiceVault.AudioPlugin` as the startup project.

2. Run the project using Visual Studio or the .NET CLI:
    ```sh
    dotnet run --project VoiceVault.AudioPlugin
    ```

## Usage

### Audio Recording

The `AudioRecorderPage` and `AudioRecorderPageViewModel` demonstrate how to record audio in the background. The recording options can be configured, and the recorded audio can be played back.

### Audio Playback

The `MusicPlayerPage` and `MusicPlayerPageViewModel` demonstrate how to play, pause, stop, and seek audio files. The playback speed can also be adjusted.
