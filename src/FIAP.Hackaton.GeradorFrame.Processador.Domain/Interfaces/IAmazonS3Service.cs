namespace FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;

public interface IAmazonS3Service
{
    Task<string> GerarURLPreAssunadaAsync(string bucketName,
        string objectKey);

    Task UploadFileToS3Async(string bucketName, string keyName, string filePath);

    Task<string> DownloadFileFromS3Async(string bucketName, string keyName, string tempVideoPath);
}
