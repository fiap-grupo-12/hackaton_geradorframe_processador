using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Api;
using FIAP.Hackaton.GeradorFrame.Processador.Application.Model;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Infra.Mensageria;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAP.Hackaton.ProcessarVideo.Api
{
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public class Function(IServiceProvider serviceProvider)
    {
        private readonly IProcessarVideoUseCase _processarVideoUseCase = serviceProvider.GetService<IProcessarVideoUseCase>();
        private readonly IBuscarRequisitanteUseCase _buscarRequisitanteUseCase = serviceProvider.GetService<IBuscarRequisitanteUseCase>();
        private readonly IAtualizaStatusRequisitanteUseCase _atualizaStatusRequisitanteUseCase = serviceProvider.GetService<IAtualizaStatusRequisitanteUseCase>();
        private readonly IMensageriaProcessarVideo _mensageriaProcessarVideo = serviceProvider.GetService<IMensageriaProcessarVideo>();
        private readonly string _sqsNotification = Environment.GetEnvironmentVariable("sqs_enviar_notificacao_url");
        private readonly string _sqsS3Envoke = Environment.GetEnvironmentVariable("sqs_envoke_s3_url");

        public Function()
            : this(Startup.ConfigureServices())
        {
        }

        public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            context.Logger.LogInformation($"Mensagem Recebida: {JsonSerializer.Serialize(evnt)}");

            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message.S3, context);
            }
        }

        private async Task ProcessMessageAsync(S3Event.S3Entity message, ILambdaContext context)
        {
            context.Logger.LogInformation($"Processando a mensagem: {message.Object.Key}");

            //Split pra pegar o nome da pasta
            var input = new ProcessarVideoInput
            {
                IdRequisicao = Guid.Parse(message.Object.Key.Split("/")[0]),
                VideoName = message.Object.Key.Split("/")[1],
                BucketName = message.Bucket.Name,
                ArchiveKey = message.Object.Key
            };

            //Busca Requisitante
            var requisitante = await _buscarRequisitanteUseCase.Execute(input.IdRequisicao, default);
            await _atualizaStatusRequisitanteUseCase.Execute(requisitante, StatusVideo.EmProcessamento, default);

            //Processa Video
            var processado = await _processarVideoUseCase.Execute(input, default);

            await _atualizaStatusRequisitanteUseCase.Execute(requisitante, StatusVideo.Processado, default);

            //Cria notificação
            var notification = new Notification().GerarEmailJsonErro(requisitante, !processado);

            //Publica mensagem para notificação
            await _mensageriaProcessarVideo.EnviarNotificacaoAsync(_sqsNotification,
                notification);

            //Deleta mensagem da fila
            //await _mensageriaProcessarVideo.DeletarMensagemSQSAsync(_sqsS3Envoke,
            //    message.ReceiptHandle);

            context.Logger.LogInformation($"Processamento concluído: {message.Object.Key}");

            await Task.CompletedTask;
        }
    }
}