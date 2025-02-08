namespace FIAP.GeradorDeFrames.Application.Transport;

public class ProcessarVideoInput
{
    public string BucketName { get; set; }
    public string VideoName { get; set; }
    public string ArchiveKey { get; set; }
    public Guid IdRequisicao { get; set; }
    public string MessageReceiptHandle { get; set; }
}
