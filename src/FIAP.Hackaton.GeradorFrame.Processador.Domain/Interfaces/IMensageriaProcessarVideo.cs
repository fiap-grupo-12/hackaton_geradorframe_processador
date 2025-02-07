namespace FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;

public interface IMensageriaProcessarVideo
{
    Task EnviarNotificacaoAsync(string queueUrl, string body);
    Task DeletarMensagemSQSAsync(string queueUrl, string receiptHandle);
}
