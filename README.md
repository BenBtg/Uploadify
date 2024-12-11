# VoiceVault Repository

This repository contains the source code for the VoiceVault project, which includes both a backend API and a cross-platform mobile application. The two most important projects in this repository are:

1. **VoiceVault.API**: This is the backend service that handles large file uploads using chunking. It needs to be run first using the command `dotnet run` to start the API server.
2. **VoiceVault.Maui**: This is the cross-platform mobile application built using .NET MAUI. It interacts with the VoiceVault.API to upload large files from Android and iOS devices.

## Getting Started

1. Clone the repository:
    ```sh
    git clone https://github.com/benbtg/VoiceVault.git
    ```

2. Navigate to the `VoiceVault.API` directory and run the API server:
    ```sh
    cd VoiceVault.API
    dotnet run
    ```

3. Open the `VoiceVault.Maui` solution in Visual Studio 2022, restore the NuGet packages, and deploy the app to your Android or iOS device/emulator.

## Projects

- **VoiceVault.API**: Handles large file uploads using chunking. It provides endpoints for uploading file chunks and completing the upload process.
- **VoiceVault.Maui**: A mobile app that allows users to select and upload large files to the VoiceVault.API. It provides progress tracking and estimated time to completion for uploads.
