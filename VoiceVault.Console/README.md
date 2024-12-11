# VoiceVault.Console

## Overview

The VoiceVault.Console application is a console-based utility designed to demonstrate and test the functionality of uploading large files in chunks to a web API. This application is particularly useful for scenarios where large files need to be uploaded reliably, with support for resuming interrupted uploads.

## Features

- **Chunked File Upload**: Splits large files into smaller chunks and uploads each chunk sequentially to the server.
- **Progress Tracking**: Tracks and displays the progress of each chunk upload.
- **Error Handling**: Handles errors during the upload process and provides feedback on failed chunks.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.
- A running instance of the web API that accepts file uploads. The API should be accessible at `http://localhost:5171/api/upload`.

## Usage

1. **Clone the Repository**:
   ```sh
   git clone https://github.com/yourusername/VoiceVault.Console.git
   cd VoiceVault.Console
   ```

2. **Build the Application**:
   ```sh
   dotnet build
   ```

3. **Run the Application**:
   ```sh
   dotnet run
   ```

## Configuration

The application uses hardcoded values for the API URL and chunk size. You can modify these values in the 

Program.cs

 file:

```csharp
var apiUrl = "http://localhost:5171/api/upload";
var completeUrl = "http://localhost:5171/api/complete";
var chunkSize = 1024 * 1024; // 1 MB
```

## How It Works

1. **File Preparation**:
   - The application generates a temporary file of 10 MB filled with random data for testing purposes.
   - The file is saved to a temporary location on the disk.

2. **Chunked Upload**:
   - The file is split into chunks of 1 MB each.
   - Each chunk is read from the file and uploaded to the server sequentially.
   - The server endpoint for uploading chunks is specified by 

apiUrl

.

3. **Progress and Error Handling**:
   - The application tracks the progress of each chunk upload.
   - If a chunk upload fails, the application logs the error and stops further uploads.

## Example Output

```sh
Chunk 1 uploaded successfully.
Chunk 2 uploaded successfully.
Chunk 3 upload failed: Internal Server Error
```

## Troubleshooting

- **Connection Issues**: Ensure that the web API is running and accessible at the specified URL.
- **Permissions**: Make sure the application has the necessary permissions to read and write files on the disk.
- **Network Configuration**: If running on an Android emulator, ensure that the API URL is set to `http://10.0.2.2:5171/api/upload`.
