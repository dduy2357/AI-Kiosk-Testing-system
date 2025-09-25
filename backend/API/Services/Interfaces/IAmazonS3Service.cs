namespace API.Services.Interfaces
{
    public interface IAmazonS3Service
    {
        public Task<string> UploadFileAsync(string key, IFormFile? file);
        public Task<List<string>> UploadFilesAsync(string key, List<IFormFile>? files);
        public Task<bool> DoesFileExistAsync(string fileKey);
        public Task<object> DeleteFileAsync(string fileKey);
        public Task<bool> DeleteFolderAsync(string folderKey);
        public Task<Stream> DownloadFileAsync(string s3Key);
    }
}
