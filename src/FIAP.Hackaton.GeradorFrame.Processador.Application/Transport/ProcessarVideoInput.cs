namespace FIAP.GeradorDeFrames.Application.Transport;

public class ProcessarVideoInput
{
    public string BucketName { get; set; }
    public string KeyName { get; set; }
    public Guid IdRequisicao { get; set; }
    public string MessageReceiptHandle { get; set; }
}
