using Amazon.SQS.Model;
using Amazon.SQS;
using Amazon;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;

namespace FIAP.Hackaton.ProcessarVideo.Infra.Mensageria
{
    public class MensageriaProcessarVideo : IMensageriaProcessarVideo
    {
        private readonly IAmazonSQS _amazonSQS;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.SAEast1; // SP

        public MensageriaProcessarVideo(IAmazonSQS amazonSQS)
        {
            _amazonSQS = new AmazonSQSClient(bucketRegion);
        }

        public async Task EnviarNotificacaoAsync(string queueUrl, string body)
        {
            var message = new SendMessageRequest()
            {
                QueueUrl = queueUrl,
                MessageBody = body
            };
            try
            {
                await _amazonSQS.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar a mensagem: {ex}");
            }
        }

        public async Task DeletarMensagemSQSAsync(string queueUrl, string receiptHandle)
        {
            var message = new DeleteMessageRequest()
            {
                QueueUrl = queueUrl,
                ReceiptHandle = receiptHandle
            };
            await _amazonSQS.DeleteMessageAsync(message);
        }
    }
}
