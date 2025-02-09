provider "aws" {
  region = "sa-east-1"
}

terraform {
  backend "s3" {
    bucket = "tfstate-grupo12-fiap-2025"
    key    = "lambda_processador/terraform.tfstate"
    region = "sa-east-1"
  }
}

# Definir a fila SQS
data "aws_sqs_queue" "sqs_processar_arquivo" {
  name = "sqs_processar_arquivo"
}

data "aws_sqs_queue" "sqs_notificacao" {
  name = "sqs_notificacao"
}

resource "aws_iam_role" "lambda_execution_processador_role" {
  name = "lambda_processador_execution_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      },
    ]
  })
}

resource "aws_iam_policy" "lambda_processador_policy" {
  name        = "lambda_processador_policy"
  description = "IAM policy for Lambda execution"
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents",
          "dynamodb:DeleteItem",
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:UpdateItem",
          "dynamodb:DescribeTable",
          "sqs:*",
          "s3:*"
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_execution_policy" {
  role       = aws_iam_role.lambda_execution_processador_role.name
  policy_arn = aws_iam_policy.lambda_processador_policy.arn
}

# Defina a camada Lambda

resource "aws_lambda_function" "lambda_processador_function" {
  function_name = "lambda_processador_function"
  role          = aws_iam_role.lambda_execution_processador_role.arn
  runtime       = "dotnet8"
  memory_size   = 512
  timeout       = 900
  handler       = "FIAP.Hackaton.GeradorFrame.Processador.Api::FIAP.Hackaton.ProcessarVideo.Api.Function::FunctionHandler"
  # Codigo armazenado no S3
  s3_bucket = "hackathon-grupo12-fiap-code-bucket"
  s3_key    = "lambda_processador.zip"

  environment {
    variables = {
      sqs_enviar_notificacao_url = data.aws_sqs_queue.sqs_notificacao.id
      bucket_files_out           = "hackathon-grupo12-fiap-files-out-bucket"
      sqs_envoke_s3_url          = data.aws_sqs_queue.sqs_processar_arquivo.id
    }
  }
}

resource "aws_lambda_permission" "permission_lambda_processador" {
  statement_id  = "AllowSQSTrigger"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_processador_function.function_name
  principal     = "sqs.amazonaws.com"
  source_arn    = data.aws_sqs_queue.sqs_processar_arquivo.arn
}

resource "aws_lambda_event_source_mapping" "sqs_to_lambda_processar" {
  event_source_arn = data.aws_sqs_queue.sqs_processar_arquivo.arn
  function_name    = aws_lambda_function.lambda_processador_function.function_name
  batch_size       = 10
  enabled          = true
}