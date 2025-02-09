using FFMpegCore;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Resources;
public interface IFFProbeWrapper
{
    IMediaAnalysis Analyse(string filePath);
}
