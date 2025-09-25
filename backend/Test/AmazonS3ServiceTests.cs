//using Xunit;
//using Moq;
//using Amazon.S3;
//using Amazon.S3.Model;
//using API.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using API.Configurations;

//namespace API.Tests;

//public class AmazonS3ServiceTests
//{
//    private readonly Mock<IAmazonS3> _mockS3Client;
//    private readonly AmazonS3Service _service;

//    public AmazonS3ServiceTests()
//    {
//        #region Config
//        var inMemorySettings = new Dictionary<string, string>
//        {
//            {"AWS:AccessKey", "awsAccess"},
//            {"AWS:SecretKey", "awsSecret"},
//            {"AWS:BucketName", "bucket"},
//            {"AWS:Region", "region"},
//            {"AWS:UrlKey", "urlKey"},
//        };

//        var configuration = new ConfigurationBuilder()
//            .AddInMemoryCollection(inMemorySettings)
//            .Build();
//        ConfigManager.CreateManager(configuration);
//        #endregion
//        // Fake ConfigManager để không bị null
//        ConfigManager.gI().AWSSecretKey = "awsSecret";
//        ConfigManager.gI().AWSAccessKey = "awsAccess";
//        ConfigManager.gI().AWSBucketName = "bucket";
//        ConfigManager.gI().AWSRegion = "region";
//        ConfigManager.gI().UrlS3Key = "urlKey";

//        _mockS3Client = new Mock<IAmazonS3>();
//        _service = new AmazonS3Service(_mockS3Client.Object, "my-bucket");
//    }

//    [Fact]
//    public async Task UploadFileAsync_Success_ReturnsUrl()
//    {
//        var file = new Mock<IFormFile>();
//        file.Setup(f => f.Length).Returns(10);
//        file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[10]));
//        file.Setup(f => f.ContentType).Returns("image/png");
//        _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });
//        var url = await _service.UploadFileAsync("testkey", file.Object);
//        Assert.Contains(".s3.amazonaws.com/testkey", url);
//    }

//    [Fact]
//    public async Task UploadFileAsync_Error_ThrowsException()
//    {
//        var file = new Mock<IFormFile>();
//        file.Setup(f => f.Length).Returns(10);
//        file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[10]));
//        file.Setup(f => f.ContentType).Returns("image/png");
//        _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new Exception("S3 error"));
//        var ex = await Assert.ThrowsAsync<Exception>(() => _service.UploadFileAsync("testkey", file.Object));
//        Assert.Contains("uploading to S3", ex.Message);
//    }

//    [Fact]
//    public async Task DeleteFileAsync_Success_ReturnsResponse()
//    {
//        _mockS3Client.Setup(s => s.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new DeleteObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });
//        var result = await _service.DeleteFileAsync("filekey");
//        Assert.IsType<DeleteObjectResponse>(result);
//    }

//    [Fact]
//    public async Task DeleteFileAsync_Error_ReturnsErrorString()
//    {
//        _mockS3Client.Setup(s => s.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new AmazonS3Exception("delete error"));
//        var result = await _service.DeleteFileAsync("filekey");
//        Assert.Contains("Error deleting file", result.ToString());
//    }

//    [Fact]
//    public async Task DoesFileExistAsync_Exists_ReturnsTrue()
//    {
//        _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new GetObjectMetadataResponse());
//        var exists = await _service.DoesFileExistAsync("filekey");
//        Assert.True(exists);
//    }

//    [Fact]
//    public async Task DoesFileExistAsync_NotFound_ReturnsFalse()
//    {
//        _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new AmazonS3Exception("not found") { StatusCode = System.Net.HttpStatusCode.NotFound });
//        var exists = await _service.DoesFileExistAsync("filekey");
//        Assert.False(exists);
//    }

//    [Fact]
//    public async Task DownloadFileAsync_Success_ReturnsStream()
//    {
//        var ms = new MemoryStream(new byte[10]);
//        var response = new GetObjectResponse { ResponseStream = ms };
//        _mockS3Client.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(response);
//        var stream = await _service.DownloadFileAsync("filekey");
//        Assert.IsType<MemoryStream>(stream);
//    }
//} 