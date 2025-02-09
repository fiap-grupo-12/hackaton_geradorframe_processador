using FIAP.Hackaton.GeradorFrame.Processador.Application.Helpers;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Services;

public class ZipFilesHelperTests
{
    [Fact]
    public void ZipFolder_DeveCriarArquivo_QuandoPastaEncontrada()
    {
        // Arrange
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var zipFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        Directory.CreateDirectory(tempFolder);
        File.WriteAllText(Path.Combine(tempFolder, "test.txt"), "Test content");

        try
        {
            // Act
            var result = ZipFilesHelper.ZipFolder(tempFolder, zipFilePath);

            // Assert
            Xunit.Assert.True(File.Exists(result));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);
        }
    }

    [Fact]
    public void ZipFolder_ThrowDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var zipFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");

        // Act & Assert
        var exception = Xunit.Assert.Throws<DirectoryNotFoundException>(() => ZipFilesHelper.ZipFolder(nonExistentFolder, zipFilePath));
        Xunit.Assert.Contains("Pasta não encontrada", exception.Message);
    }
}
