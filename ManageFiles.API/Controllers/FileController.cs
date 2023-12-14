using ManageFiles.API.Attributes;
using ManageFilesUtility.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PnP.Framework.Modernization.Cache;
using System.IO;
using System.Net;

namespace ManageFiles.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiKey]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, IConfiguration configuration, ILogger<FileController> logger)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //Blob
        [HttpPost("UploadBlob")]
        public async Task<ActionResult> UploadBlob(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    _logger.LogError("Invalid requst");
                    return BadRequest("Invalid requst");
                }
                var result = await _fileService.UploadBlobAsync(file.OpenReadStream(), file.FileName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DownloadBlob")]
        public async Task<ActionResult> DownloadBlob(string fileUrl)
        {
            //try
            //{
                var result = await _fileService.DownloadBlobAsync(fileUrl);
                if (result == null)
                {
                    _logger.LogError("File don't exist or Download failed");
                    return BadRequest("File don't exist or Download failed");
                }
                //return File(result.ToByteArray(), "application/octet-stream", fileName);
                return new FileStreamResult(new MemoryStream(result.FileContents), "application/octet-stream")
                {
                    FileDownloadName = result.FileName
                };
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);
            //    return BadRequest(new {status = HttpStatusCode.BadRequest, message = ex.Message });
            //}
        }

        //SFTP
        [HttpPost("UploadSftpFile")]
        public ActionResult UploadSftpFile(IFormFile file, string directory) 
        {
            try
            {
                if (file == null)
                {
                    _logger.LogError("Invalid requst");
                    return BadRequest("Invalid requst");
                }
                var result =_fileService.UploadToSftp(file.OpenReadStream(), directory, file.FileName);

                _logger.LogInformation("Upload successful");
                return Ok("Upload successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DownloadSftpFile")]
        public ActionResult DownloadSftpFile(string directory, string fileName)
        {
            try
            {
                var result = _fileService.DownloadSftp(directory, fileName);
                if (result == null)
                {
                    _logger.LogError("File don't exist or Download failed");
                    return BadRequest("File don't exist or Download failed");
                }
                //return File(result.ToByteArray(), "application/octet-stream", fileName);
                return new FileStreamResult(result, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        //Zip file
        [HttpPost("ZipFile")]
        public ActionResult ZipFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    _logger.LogError("Invalid requst");        
                    return BadRequest("Invalid requst");
                }

                var result = _fileService.ZipFile(file.OpenReadStream(), file.FileName);

                if (result == null)
                {
                    _logger.LogError("Archive failed");
                    return BadRequest("Archive failed");
                }

                _logger.LogInformation("File Archived successfully");
                result.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(result, "application/octet-stream")
                {
                    FileDownloadName = file.FileName + ".zip"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ExtractZipFile")]
        public ActionResult ExtractZipFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    _logger.LogError("Invalid requst");
                    return BadRequest("Invalid requst");
                }

                if (!_fileService.IsZipStream(file.OpenReadStream()))
                {
                    _logger.LogError("Invalid format, The file is not zip.");
                    return BadRequest("Invalid format, The file is not zip.");
                }

                var result = _fileService.ExtractZipFile(file.OpenReadStream());

                if (result == null || result.Count == 0)
                {
                    _logger.LogError("File extraction failed");
                    return BadRequest("File extraction failed");
                }

                _logger.LogInformation($"Zip file contains: {result.Count} files");
                return Ok(result.Keys.Aggregate((a, b)=> a + ", \n" + b));
                //return new FileStreamResult(result.Values.First(), "application/octet-stream")
                //{
                //    FileDownloadName = result.Keys.First(),
                //};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
