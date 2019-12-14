using System.Drawing;

namespace NAPS2.Scan.Images
{
    public class ScannedImageFactory : IScannedImageFactory
    {
        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new ScannedImage(img, bitDepth, highQuality);
        }
    }
}