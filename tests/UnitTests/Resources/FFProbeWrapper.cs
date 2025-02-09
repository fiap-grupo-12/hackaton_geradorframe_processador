using FFMpegCore;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Resources;

public class FFProbeWrapper : IFFProbeWrapper
{
    public IMediaAnalysis Analyse(string filePath)
    {
        return FFProbe.Analyse(filePath);
    }
}
