dotnet restore
dotnet build --configuration Release
dotnet lambda package --configuration Release --output-package ./pacote.zip
aws s3 cp pacote.zip s3://hackathon-grupo12-fiap-files-in-bucket-cesar2/
aws lambda update-function-code `
    --function-name ProcessadoArquivo `
    --s3-bucket hackathon-grupo12-fiap-files-in-bucket-cesar2 `
    --s3-key pacote.zip

aws lambda update-function-configuration --function-name ProcessadoArquivo --timeout 900
