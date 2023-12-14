using ManageFilesUtility.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManageFiles.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ILogger<PdfController> _logger;
        private readonly IPdfService _pdfService;

        public PdfController(ILogger<PdfController> logger, IPdfService pdfService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        }

        [HttpPost("GeneratePdf")]
        public ActionResult GeneratePdf()
        {
            try
            {
                var result = _pdfService.GeneratePdf();
                if (result == null)
                {
                    return BadRequest("Failed to generate pdf");
                }
                result.Seek(0, SeekOrigin.Begin);
                return File(result.ToArray(), "application/pdf", "PdfDoc.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
