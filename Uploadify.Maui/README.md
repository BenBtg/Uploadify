# Uploadify.Maui

Uploadify.Maui is a cross-platform application built using .NET MAUI that demonstrates large file uploads on Android and iOS using chunking via `MultipartFormDataContent`. This app is designed to handle large file uploads by splitting the file into smaller chunks and uploading each chunk sequentially.

## Features

- Large file (e.g. large audio recording) uploads using chunking
- Progress tracking for file uploads
- Estimated time to completion for uploads
- Cross-platform support for Android and iOS

## Requirements

- .NET 9.0 or later
- Visual Studio 2022 or later with .NET MAUI workload installed

## Installation

1. Clone the repository:
2. Open the solution in Visual Studio 2022.
3. Restore the NuGet packages.
4. Deploy the app to your Android or iOS device/emulator.

## Usage

1. Launch the app on your Android or iOS device.
2. Select a large file to upload.
3. Monitor the upload progress and estimated time to completion.

## Notes

- The file upload feature uses chunking to handle large files efficiently.
- The upload process works only in the foreground for both Android and iOS platforms.
- Ensure that the API base URL is correctly configured in the `UploadService` class based on the platform.
