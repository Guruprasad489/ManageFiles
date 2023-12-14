using Azure.Storage;
using Azure.Storage.Blobs;
using ManageFilesUtility.Interfaces;
using ManageFilesUtility.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Org.BouncyCastle.Utilities.Bzip2;
using PnP.Framework.Modernization.Cache;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //Blob storage methods
        public async Task<string> UploadBlobAsync(Stream stream, string fileName)
        {
            try
            {
                string blobConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
                string blobContainerName = "sample-container";

                BlobContainerClient blobContainerClient = new BlobContainerClient(blobConnectionString, blobContainerName);
                await blobContainerClient.CreateIfNotExistsAsync();
                BlobClient blobClient = blobContainerClient.GetBlobClient($"UploadedFiles/{fileName}");

                stream.Seek(0, SeekOrigin.Begin);
                await blobClient.UploadAsync(stream);

                _logger.LogInformation($"File {fileName} successfully uploaded, FileUrl: {blobClient.Uri.AbsoluteUri}");
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }
        }

        public async Task<CustomFile?> DownloadBlobAsync(string fileUrl)
        {
            string accountName = "devstoreaccount1";
            string accountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
            StorageSharedKeyCredential keyCredential = new StorageSharedKeyCredential(accountName, accountKey);
            BlobClient blob = new BlobClient(new Uri(fileUrl), keyCredential);

            using (var stream = new MemoryStream())
            {
                var isBlobExist = await blob.ExistsAsync();
                if (!isBlobExist)
                {
                    _logger.LogError("Blob not found");
                    return null;
                }

                await blob.DownloadToAsync(stream);

                if (stream.Length == 0)
                {
                    _logger.LogError("Failed to download");
                    return null;
                }

                stream.Seek(0, SeekOrigin.Begin);
                var contentType = (await blob.GetPropertiesAsync()).Value.ContentType;
                _logger.LogInformation($"File downloaded successfully");

                
                return new CustomFile
                {
                    FileContents = stream.ToArray(),
                    ContentType = contentType,
                    FileName = blob.Name
                };
            }
        }

        //SFTP Server Methods
        public string UploadToSftp(Stream inputStream, string directory, string fileName)
        {
            string host = "eu-central-1.sftpcloud.io";
            int port = 22;
            string userName = "494b9927d9a44301b0e4f44cea364cc8";
            string password = "Ustzrk7nuzESg3OlqPpswmm2vE93nFgd";

            using (SftpClient client = new SftpClient(host, port, userName, password))
            {
                client.Connect();
                client.UploadFile(inputStream, Path.Combine(directory, fileName));
            }
            return "Upload successful";
        }

        public Stream? DownloadSftp(string directory, string fileName)
        {
            string host = "eu-central-1.sftpcloud.io";
            int port = 22;
            string userName = "494b9927d9a44301b0e4f44cea364cc8";
            string password = "Ustzrk7nuzESg3OlqPpswmm2vE93nFgd";

                
            Stream stream = new MemoryStream();
            
            using (SftpClient client = new SftpClient(host, port, userName, password))
            {
                client.Connect();

                if(!client.Exists(Path.Combine(directory, fileName)))
                {
                    return null;
                }

                client.DownloadFile(Path.Combine(directory, fileName), stream);

                if (stream.Length == 0)
                {
                    return null;
                }

                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        //Zip file methods
        public Stream ZipFile(Stream stream, string name)
        {
            try
            {
                MemoryStream compressedStream = new MemoryStream();

                using (ZipArchive archive = new ZipArchive(compressedStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    ZipArchiveEntry entry = archive.CreateEntry(name);

                    using (Stream entryStream = entry.Open())
                    {
                        stream.CopyTo(entryStream);
                    }
                    compressedStream.Seek(0, SeekOrigin.Begin);

                    return compressedStream;
                }


            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error compressing stream to zip: {ex.Message}");
                return null;
            }
        }

        public Dictionary<string, Stream> ExtractZipFile(Stream zipStream)
        {
            Dictionary<string, Stream> list = new Dictionary<string, Stream>();

            ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read, true);

            foreach (var item in zipArchive.Entries)
            {
                list.Add(item.FullName, item.Open());
            }
            return list;
        }

        public bool IsZipStream(Stream stream)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    byte[] headerBytes = br.ReadBytes(4);
                    return headerBytes.Length >= 4 &&
                           headerBytes[0] == 0x50 &&
                           headerBytes[1] == 0x4B &&
                           headerBytes[2] == 0x03 &&
                           headerBytes[3] == 0x04;
                }
            }
            catch (IOException)
            {
                return false; // Handle the case where the stream cannot be read.
            }
        }

        public string UploadToSharePoint(Stream stream, string fileName, string folderName)
        {
            string url = "";
            string appId = "";
            string appSecret = "";
            string userName = "";
            string password = "";

            var securedPassword = new System.Security.SecureString();
            foreach (char c in password)
            {
                securedPassword.AppendChar(c);
            }

            //var ctx = new PnP.Framework.AuthenticationManager().GetOnPremisesContext(url, userName, securedPassword);
            var ctx = new PnP.Framework.AuthenticationManager().GetACSAppOnlyContext(url, appId, appSecret);
            ctx.ExecuteQuery();

            var web = ctx.Web;
            var list = web.Lists.GetByTitle("Documents");

            var spFolder = list.RootFolder.EnsureFolder(folderName);

            var fileCreationInfo = new FileCreationInformation
            {
                ContentStream = stream,
                Url = fileName,
                Overwrite = true
            };
            var spFIle = spFolder.Files.Add(fileCreationInfo);
            ctx.Load(spFIle);
            ctx.ExecuteQuery();

            return "Upload Successful";
        }
    }
}