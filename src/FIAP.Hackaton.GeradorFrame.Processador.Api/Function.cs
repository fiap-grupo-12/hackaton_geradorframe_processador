using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAP.Hackaton.ProcessarVideo.Api;

public class Function(IProcessarVideoUseCase processarVideoUseCase)
{
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Mensagem processada {message.Body}");

        var processarVideoEvento = JsonSerializer.Deserialize<ProcessarVideoEvento>(message.Body);

        //Split pra pegar o nome da pasta
        var input = new ProcessarVideoInput
        {
            KeyName = processarVideoEvento.Key,
            IdRequisicao = processarVideoEvento.IdRequisicao,
            BucketName = Environment.GetEnvironmentVariable("bucket_files_in"),
            MessageReceiptHandle = message.ReceiptHandle
        };

        await processarVideoUseCase.Execute(input, default);

        context.Logger.LogInformation($"Processamento concluído. {message.Body}");

        await Task.CompletedTask;
    }


}