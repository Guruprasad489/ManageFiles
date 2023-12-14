using ManageFilesUtility.Interfaces;
using ManageFilesUtility.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PgpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Services
{
    public class PgpService : IPgpService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PgpService> _logger;

        public PgpService(IConfiguration configuration, ILogger<PgpService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public PgpKeys GetPgpKeys()
        {
            //Read Keys from configuration file
            return new PgpKeys();
        }

        public async Task<Stream?> EncryptFile(Stream inputStream, string publicKey)
        {
            try
            {
                var pgp = new PGP(new EncryptionKeys(publicKey));


                using (Stream outputStream = new MemoryStream())
                {
                    _logger.LogInformation("File encryption started.");
                    await pgp.EncryptStreamAsync(inputStream, outputStream);

                    if (outputStream == null)
                    {
                        _logger.LogInformation("File encryption failed.");
                        return null;
                    }
                    _logger.LogInformation("File encryption ended.");
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("File encryption failed.");
                _logger.LogInformation(ex.Message);
                return null;
            }
        }

        public async Task<Stream?> DecryptFile(Stream inputStream, string privateKey, string passPhrase)
        {
            try
            {
                var pgp = new PGP(new EncryptionKeys(privateKey, passPhrase));


                using (Stream outputStream = new MemoryStream())
                {
                    _logger.LogInformation("File decryption started.");
                    await pgp.DecryptStreamAsync(inputStream, outputStream);

                    if (outputStream == null)
                    {
                        _logger.LogInformation("File decryption failed.");
                        return null;
                    }
                    _logger.LogInformation("File decryption ended.");
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("File decryption failed.");
                _logger.LogInformation(ex.Message);
                return null;
            }
        }
    }
}
