using NAPS2.Scan.Images;
using System.Collections.Generic;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<IScannedImage> Import(string filePath);
    }
}