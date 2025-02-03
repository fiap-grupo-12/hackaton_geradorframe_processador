using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;

namespace FIAP.Hackaton.ProcessarVideo.Infra.AmazonS3;

public class AmazonS3Service : IAmazonS3Service
{
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.SAEast1; // SP
    private AmazonS3Client _amazonS3Client;

    public AmazonS3Service()
    {
        _amazonS3Client = new AmazonS3Client(bucketRegion);
    }

    public async Task<string> GerarURLPreAssunadaAsync(string bucketName,
        string objectKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(15), // Expira em 15 minutos
            Verb = HttpVerb.GET
        };

        // Gerar URL
        return await _amazonS3Client.GetPreSignedURLAsync(request);
    }

    public async Task<string> DownloadFileFromS3Async(string bucketName, string keyName, string tempVideoPath)
    {
        try
        {
            // Cria uma requisição para baixar o objeto
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            // Baixa o objeto do S3
            using (var response = await _amazonS3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var fileStream = new FileStream(tempVideoPath, FileMode.Create, FileAccess.Write))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            return tempVideoPath;
        }
        catch (Exception ex)
        {
            //log de erro
            return "";
        }
    }

    public async Task UploadFileToS3Async(string bucketName, string keyName, string filePath)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            FilePath = filePath
        };

        await _amazonS3Client.PutObjectAsync(request);
    }
}
