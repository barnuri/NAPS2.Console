﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NAPS2.Scan.Images
{
    internal static class ScannedImageHelper
    {
        public static void GetSmallestBitmap(Bitmap sourceImage, ScanBitDepth bitDepth, bool highQuality, out Bitmap bitmap, out MemoryStream encodedBitmap, out ImageFormat imageFormat)
        {
            // Defaults for out arguments
            bitmap = null;
            encodedBitmap = null;
            imageFormat = ImageFormat.Png;

            // Store the image in as little space as possible
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                // Store as a 1-bit bitmap
                // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
                bitmap = (Bitmap)BitmapHelper.CopyToBpp(sourceImage, 1).Clone();
                // Note that if a black and white image comes from native WIA, bitDepth is unknown,
                // so the image will be png-encoded below instead of using a 1-bit bitmap
            }
            else if (highQuality)
            {
                // Store as PNG
                // Lossless, but some images (color/grayscale) take up lots of storage
                encodedBitmap = EncodeBitmap(sourceImage, ImageFormat.Png);
            }
            else
            {
                // Store as PNG/JPEG depending on which is smaller
                var pngEncoded = EncodeBitmap(sourceImage, ImageFormat.Png);
                var jpegEncoded = EncodeBitmap(sourceImage, ImageFormat.Jpeg);
                if (pngEncoded.Length <= jpegEncoded.Length)
                {
                    // Probably a black and white image (from native WIA, so bitDepth is unknown), which PNG compresses well vs. JPEG
                    encodedBitmap = pngEncoded;
                    jpegEncoded.Dispose();
                }
                else
                {
                    // Probably a color or grayscale image, which JPEG compresses well vs. PNG
                    encodedBitmap = jpegEncoded;
                    pngEncoded.Dispose();
                    imageFormat = ImageFormat.Jpeg;
                }
            }
        }

        private static MemoryStream EncodeBitmap(Bitmap bitmap, ImageFormat imageFormat)
        {
            var encoded = new MemoryStream();
            bitmap.Save(encoded, imageFormat);
            return encoded;
        }
    }
}