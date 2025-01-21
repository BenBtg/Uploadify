# Uploadify Repository

This repository contains the source code for the Uploadify project, which includes both a backend API and a cross-platform mobile application. The two most important projects in this repository are:

1. **Uploadify.API**: This is the backend service that handles large file uploads using chunking. It needs to be run first using the command `dotnet run` to start the API server.
2. **Uploadify.Maui**: This is the cross-platform mobile application built using .NET MAUI. It interacts with the Uploadify.API to upload large files from Android and iOS devices.

## Getting Started

1. Clone the repository:
    ```sh
    git clone https://github.com/benbtg/Uploadify.git
    ```

2. Navigate to the `Uploadify.API` directory and run the API server:
    ```sh
    cd Uploadify.API
    dotnet run
    ```

3. Open the `Uploadify.Maui` solution in Visual Studio 2022, restore the NuGet packages, and deploy the app to your Android or iOS device/emulator.

## Projects

- **Uploadify.API**: Handles large file uploads using chunking. It provides endpoints for uploading file chunks and completing the upload process.
- **Uploadify.Maui**: A mobile app that allows users to select and upload large files to the Uploadify.API. It provides progress tracking and estimated time to completion for uploads.
