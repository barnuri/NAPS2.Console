using System.Drawing;

namespace NAPS2.Scan.Images
{
    public interface IScannedImageFactory
    {
        IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality);
    }
}