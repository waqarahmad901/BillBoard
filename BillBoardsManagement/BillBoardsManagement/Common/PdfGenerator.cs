using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BillBoardsManagement.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TmsWebApp.Common
{
    public class PdfGeneratorAggrement
    {
        public static string GenerateOnflyPdf(string filePath, List<PdfCoordinatesModel> pdfCoordinates)
        {
            string oldFile = filePath;
            string newFile = Path.Combine(Path.GetDirectoryName(filePath),DateTime.Now.ToString("ddMMyyyyhhmmsstt")+".pdf");

            // open the reader
            PdfReader reader = new PdfReader(oldFile);
            var fnBoldFnt = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLACK);
        
            byte[] bytes = null;
            using (var ms = new MemoryStream())
            {
                using (PdfStamper stamper = new PdfStamper(reader, ms))
                {
                    PdfContentByte canvas = stamper.GetOverContent(1);
                
                    foreach (var item in pdfCoordinates)
                    {
                        var fnNormalFnt = FontFactory.GetFont("Arial", item.FontSize, Font.NORMAL, BaseColor.BLACK);
                        if (item.IsBold)
                            ColumnText.ShowTextAligned(canvas, item.Alignment, new Phrase(item.Text, fnBoldFnt), item.X, item.Y, 0);

                        else

                            ColumnText.ShowTextAligned(canvas, item.Alignment, new Phrase(item.Text, fnNormalFnt), item.X, item.Y, 0);
                    }
                }
                bytes = ms.ToArray();
            }
            reader.Close();
            File.WriteAllBytes(newFile, bytes);
             
            return newFile;
        }
    }
}
