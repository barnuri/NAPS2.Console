using System.Drawing;

namespace NAPS2.Scan.Images
{
    public class FileBasedScannedImageFactory : IScannedImageFactory
    {
        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new FileBasedScannedImage(img, bitDepth, highQuality);
        }
    }
}