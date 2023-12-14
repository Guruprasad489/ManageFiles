using ManageFilesUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadBlobAsync(Stream stream, string fileName);
        Task<CustomFile?> DownloadBlobAsync(string fileUrl);
        string UploadToSftp(Stream inputStream, string directory, string fileName);
        Stream? DownloadSftp(string directory, string fileName);
        Stream ZipFile(Stream stream, string name);
        Dictionary<string, Stream> ExtractZipFile(Stream zipStream);
        bool IsZipStream(Stream stream);
        string UploadToSharePoint(Stream stream, string fileName, string folderName);
    }
}
