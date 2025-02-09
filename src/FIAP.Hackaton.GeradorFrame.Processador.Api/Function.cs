using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Api;
using FIAP.Hackaton.GeradorFrame.Processador.Application.Model;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAP.Hackaton.ProcessarVideo.Api
{
    public class Function(IServiceProvider serviceProvider)
    {
        private readonly IProcessarVideoUseCase _processarVideoUseCase = serviceProvider.GetService<IProcessarVideoUseCase>();
        private readonly IBuscarRequisitanteUseCase _buscarRequisitanteUseCase = serviceProvider.GetService<IBuscarRequisitanteUseCase>();
        private readonly IAtualizaStatusRequisitanteUseCase _atualizaStatusRequisitanteUseCase = serviceProvider.GetService<IAtualizaStatusRequisitanteUseCase>();
        private readonly IMensageriaProcessarVideo _mensageriaProcessarVideo = serviceProvider.GetService<IMensageriaProcessarVideo>();
        private readonly string _sqsNotification = Environment.GetEnvironmentVariable("sqs_enviar_notificacao_url");
        private readonly string _sqsS3Envoke = Environment.GetEnvironmentVariable("sqs_envoke_s3_url");

        public Function() : this(Startup.ConfigureServices()) { }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            var client = new AmazonLambdaClient(RegionEndpoint.SAEast1);

            try
            {
                context.Logger.LogInformation($"📥 Mensagem Recebida: {JsonSerializer.Serialize(evnt)}");

                var receiptHandle = evnt.Records[0].ReceiptHandle;
                S3EventMessage s3Event = JsonSerializer.Deserialize<S3EventMessage>(evnt.Records[0].Body);

                foreach (var message in s3Event.Records)
                {
                    await ProcessMessageAsync(message.s3, receiptHandle, context);
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"❌ Erro ao processar a mensagem da fila SQS: {ex.Message}");
                context.Logger.LogError($"🛑 Stack Trace: {ex.StackTrace}");
            }
        }

        private async Task ProcessMessageAsync(S3 message, string receiptHandle, ILambdaContext context)
        {
            try
            {
                context.Logger.LogInformation($"🛠️ Iniciando processamento do arquivo: {message._object.key}");

                // Extrai informações do arquivo
                string[] pathSegments = message._object.key.Split("/");
                if (pathSegments.Length < 2)
                {
                    context.Logger.LogError($"❌ Formato inesperado de key no S3: {message._object.key}");
                    return;
                }

                var input = new ProcessarVideoInput
                {
                    IdRequisicao = Guid.Parse(pathSegments[0]),
                    VideoName = pathSegments[1],
                    BucketName = message.bucket.name,
                    ArchiveKey = message._object.key
                };

                context.Logger.LogInformation($"🔎 IdRequisicao: {input.IdRequisicao}");
                context.Logger.LogInformation($"🎥 Nome do vídeo: {input.VideoName}");
                context.Logger.LogInformation($"📂 Bucket: {input.BucketName}");

                // Busca Requisitante
                context.Logger.LogInformation("🔄 Buscando requisitante...");
                var requisitante = await _buscarRequisitanteUseCase.Execute(input.IdRequisicao, default);
                if (requisitante == null)
                {
                    context.Logger.LogError($"❌ Nenhum requisitante encontrado para ID: {input.IdRequisicao}");
                    return;
                }

                // Atualiza status para "Em Processamento"
                context.Logger.LogInformation("🔄 Atualizando status do requisitante para 'EmProcessamento'...");
                await _atualizaStatusRequisitanteUseCase.Execute(requisitante, StatusVideo.EmProcessamento, default);

                // Processa Video
                context.Logger.LogInformation("🎬 Iniciando processamento do vídeo...");
                var processado = await _processarVideoUseCase.Execute(input, context);

                var status = processado ? StatusVideo.Processado : StatusVideo.ProcessadoComErro;
                context.Logger.LogInformation($"✅ Processamento concluído para {message._object.key} com status: {status}");

                
                context.Logger.LogInformation("📨 Enviando notificação de processamento...");
                var notification = new Notification().GerarEmailJsonErro(requisitante, !processado);
                await _mensageriaProcessarVideo.EnviarNotificacaoAsync(_sqsNotification, notification);

                context.Logger.LogInformation("🗑️ Removendo mensagem da fila...");
                await _mensageriaProcessarVideo.DeletarMensagemSQSAsync(_sqsS3Envoke, receiptHandle);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"❌ Erro durante o processamento do arquivo {message._object.key}: {ex.Message}");
                context.Logger.LogError($"🛑 Stack Trace: {ex.StackTrace}");
            }
        }
    }
}