using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VoiceVaultAPI.Controllers;

namespace VoiceVaultAPI.Tests
{
    public class VoiceVaultControllerTests
    {
        private readonly VoiceVaultController.FileUploadController _controller;

        public VoiceVaultControllerTests()
        {
            _controller = new VoiceVaultController.FileUploadController();
        }

        [Fact]
        public async Task Upload_ReturnsBadRequest_WhenNoFileUploaded()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.Upload(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task Upload_ReturnsOk_WhenFileUploaded()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);

            // Act
            var result = await _controller.Upload(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File uploaded successfully (simulated).", okResult.Value);
        }
    }
}
