using System.Drawing;

namespace NAPS2.Ocr
{
    public interface IOcrEngine
    {
        bool CanProcess(string langCode);

        OcrResult ProcessImage(Image image, string langCode);
    }
}