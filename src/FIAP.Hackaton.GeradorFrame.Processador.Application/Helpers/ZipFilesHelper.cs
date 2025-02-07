using System.IO.Compression;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.Helpers;

public class ZipFilesHelper
{
    public static string ZipFolder(string folderPath, string zipFilePath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Pasta não encontrada: {folderPath}");
        }

        ZipFile.CreateFromDirectory(folderPath, zipFilePath);
        return zipFilePath;
    }
}
