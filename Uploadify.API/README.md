# Uploadify.API

Uploadify.API is a backend service designed to handle large file uploads efficiently using chunking. This API works in conjunction with the Uploadify.Maui app to enable seamless and reliable file uploads on Android and iOS devices.

## Features

- Supports large file uploads using chunking
- Ensures data integrity and reliability
- Provides progress tracking and estimated time to completion

## How It Works

1. **Chunking**: The file to be uploaded is divided into smaller chunks (e.g., 1 MB each).
2. **Sequential Upload**: Each chunk is uploaded sequentially to the server using `MultipartFormDataContent`.
3. **Progress Tracking**: The client app tracks the progress of the upload and provides feedback to the user.
4. **Completion Notification**: Once all chunks are uploaded, the client notifies the server that the upload is complete.

## API Endpoints

- `POST /api/upload`: Endpoint to upload a file chunk.
- `POST /api/complete`: Endpoint to notify the server that the file upload is complete.

## Requirements

- .NET 9.0 or later
- ASP.NET Core Web API

## Installation

1. Clone the repository:
2. Open the solution in Visual Studio 2022.
3. Restore the NuGet packages.
4. Run the API project.

## Usage

1. Configure the Uploadify.Maui app to point to the running API.
2. Use the app to select and upload a large file.
3. Monitor the upload progress and completion status.