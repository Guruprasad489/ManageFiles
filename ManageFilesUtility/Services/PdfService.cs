using ManageFilesUtility.Interfaces;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageFilesUtility.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public MemoryStream GeneratePdf()
        {
            try
            {
                var document = new Document();

                var pageSetup = document.DefaultPageSetup.Clone();
                pageSetup.Orientation = Orientation.Landscape;
                pageSetup.TopMargin = Unit.FromCentimeter(2.5);
                pageSetup.BottomMargin = Unit.FromCentimeter(2);
                pageSetup.LeftMargin = Unit.FromCentimeter(1);
                pageSetup.RightMargin = Unit.FromCentimeter(1);

                Section section = document.AddSection();
                section.PageSetup = pageSetup;
                section.PageSetup.PageFormat = PageFormat.A4;
                //var sectionWidth = 21; //for Portrait 
                var sectionWidth = 27.7; //for Landscape

                Paragraph header = section.Headers.Primary.AddParagraph("PDF Document");
                header.Format.Font.Size = 16;
                header.Format.Font.Bold = true;
                header.Format.Alignment = ParagraphAlignment.Center;

                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.AddText("Page ");
                footer.AddPageField();
                footer.Format.Font.Size = 10;
                footer.Format.Alignment = ParagraphAlignment.Right;

                var borderlessTable = section.AddTable();
                borderlessTable.Format.Borders.ClearAll();
                borderlessTable.AddColumn(Unit.FromCentimeter(sectionWidth/2));
                borderlessTable.AddColumn(Unit.FromCentimeter(sectionWidth/2));
                var row1 = borderlessTable.AddRow();
                var row2 = borderlessTable.AddRow();
                borderlessTable.Section.AddParagraph("");

                row1.Cells[0].AddParagraph("Name: Guruprasad");
                row1.Cells[1].AddParagraph("Place: Bangalore");
                row2.Cells[0].AddParagraph("XYZ: xyz");
                row2.Cells[1].AddParagraph("ABC: abc");

                var table = section.AddTable();
                table.Borders.Width = 0.25;

                for (int i = 0; i < 10; i++)
                {
                    table.AddColumn(Unit.FromCentimeter(sectionWidth/10));
                }

                var thead = table.AddRow();
                thead.HeadingFormat = true;
                thead.Format.Font.Bold = true;
                thead.Shading.Color = Colors.LightGray;

                for (int i = 0; i < 10; i++)
                {
                    thead.Cells[i].AddParagraph($"Header {i}");
                }

                for (int i = 0; i < 50; i++)
                {
                    var row = table.AddRow();
                    for (int j = 0; j < 10; j++)
                    {
                        row.Cells[j].AddParagraph($"Data {i}{j}");
                    }
                }

                var pdfRenderer = new PdfDocumentRenderer();
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();

                var stream = new MemoryStream();
                pdfRenderer.PdfDocument.Save(stream, false);
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
