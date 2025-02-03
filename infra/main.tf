provider "aws" {
  region = "us-east-1"
}

terraform {
  backend "s3" {
    bucket = "tfstate-grupo12-fiap-2025"
    key    = "lambda_processador/terraform.tfstate"
    region = "sa-east-1"
  }
}

resource "aws_iam_role" "lambda_execution_role" {
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

resource "aws_iam_policy" "lambda_policy" {
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
		  "sqs:*"
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_execution_policy" {
  role       = aws_iam_role.lambda_execution_role.name
  policy_arn = aws_iam_policy.lambda_policy.arn
}

resource "aws_lambda_function" "processar_video_function" {
  function_name = "lambda_processador_function"
  role          = aws_iam_role.lambda_execution_role.arn
  runtime       = "dotnet8"
  memory_size   = 512
  timeout       = 30
  handler       = "FIAP.Hackaton.GeradorFrame.Processador.Api.Function_Handler_Generated::FunctionHandler"
  # Código armazenado no S3
  s3_bucket = "code-lambdas-functions"
  s3_key    = "lambda_processar_video_function.zip"
  
  environment {
    variables = {
	  "bucket_files_in" = "",
	  "bucket_files_out" = "",
	  "subject_email_notificacao" = "Notificação de requisição de Frames",
	  "sqs_processar_video_url" = "data.aws_sqs_queue.processar_video.id",
	  "sqs_enviar_notificacao_url" = "data.aws_sqs_queue.enviar_notificacao.id"
    }
  }
}

}