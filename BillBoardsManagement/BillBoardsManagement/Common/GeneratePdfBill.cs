using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using BillBoardsManagement.Repository;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Font = iTextSharp.text.Font;

namespace BillBoardsManagement.Common
{
    public class PdfGenerator
    {
        public static string GenerateOnflyPdf(string filePath, IEnumerable<Customer> customers, IEnumerable<lk_rates> allrates)
        {
            string oldFile = filePath;

            FileStream fs = new FileStream(filePath, FileMode.Create);

            Document document = new Document(PageSize.A4, 25, 25, 30, 30);
            // Create an instance to the PDF file by creating an instance of the PDF 
            // Writer class using the document and the filestrem in the constructor.
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();
            BaseFont fCb = BaseFont.CreateFont("c:\\windows\\fonts\\calibrib.ttf", BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetFontAndSize(fCb, 9);
            var headerFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLACK);
            Paragraph header = new Paragraph("PARKS & HORTICULTURE AUTHORITY RAWALPINDI.", headerFont) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph1 = new Paragraph("RAJA ZAHID LATIF CONTRACTOR ADVERTISEMENT FEE 2017.", FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph2 = new Paragraph("PHOTOHAR TOWN AREA.", FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph3 = new Paragraph("BILL.", FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph4 = new Paragraph("ALLIED BANK LTD.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_LEFT };
            Paragraph paragraph5 = new Paragraph("ADYALA ROAD RAWALPINDI.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_LEFT };

            document.Add(header);
            document.Add(paragraph1);
            document.Add(paragraph2);
            document.Add(paragraph3);
            document.Add(paragraph4);
            document.Add(paragraph5);

            var fntTableFontHdr = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK);
            var fntTableFontRow = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK);
            var table = new PdfPTable(14)
            {
                WidthPercentage = 100,
                SpacingBefore = 20 ,
                DefaultCell = { Padding = 5},
               
            };

            table.SetWidths(new int[] {60,150,50,50,50,50,50,50,50,50,80,80,80,150});
            table.AddCell(new PdfPCell(new Phrase("SR NO.", fntTableFontHdr)) {  HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("LOCATION", fntTableFontHdr)) {  HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE }); 
            table.AddCell(new PdfPCell(new Phrase("TYPE OF ADVERTISEMENT", fntTableFontHdr)) { Rotation = 90, FixedHeight = 100, VerticalAlignment = Element.ALIGN_MIDDLE,HorizontalAlignment = Element.ALIGN_CENTER, PaddingBottom = 5 });
            PdfPCell cell = new PdfPCell(new Paragraph("MEASURMENT", fntTableFontHdr) {Alignment = Element.ALIGN_CENTER}) {Colspan = 7,HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
            table.AddCell(cell); 
            table.AddCell(new PdfPCell(new Phrase("TOTAL MEASURMENT", fntTableFontHdr)) { Rotation = 90,VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, PaddingBottom = 5});
         
            table.AddCell(new PdfPCell(new Phrase("RATE PER SQ.FT PER ANUM", fntTableFontHdr)) { Rotation = 90,VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("AMOUNT", fntTableFontHdr)) { Rotation = 90,VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });

            table.AddCell(new PdfPCell(new Phrase("IMAGE", fntTableFontHdr)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            int row = 1;
            decimal totalAmount = 0;
            foreach (var item in customers)
            {  
                table.AddCell(new Phrase(row++ + "", fntTableFontRow));
                table.AddCell(new Phrase(item.Location, fntTableFontRow)); 
                table.AddCell(new Phrase(item.Type, fntTableFontRow)); 
                table.AddCell(new Phrase(item.Size1, fntTableFontRow)); 
                table.AddCell(new Phrase("X", fntTableFontRow)); 
                table.AddCell(new Phrase(item.Size2, fntTableFontRow)); 
                table.AddCell(new Phrase("X", fntTableFontRow));
                table.AddCell(new Phrase(item.Size3, fntTableFontRow));
                table.AddCell(new Phrase("X", fntTableFontRow));
                table.AddCell(new Phrase(item.Size4, fntTableFontRow));
                table.AddCell(new Phrase(item.TotalMeasurment, fntTableFontRow));
                decimal perAnumRate = allrates.Where(x => x.Type == item.Type).Select(x => x.Rate).FirstOrDefault() *12;
                table.AddCell(new Phrase(perAnumRate + "", fntTableFontRow));
                decimal amount = perAnumRate * decimal.Parse(item.TotalMeasurment);
                table.AddCell(new Phrase(amount.ToString("0.00") + "", fntTableFontRow));
                totalAmount += amount;
                if (item.Picture != null)
                {
                    var img = iTextSharp.text.Image.GetInstance(item.Picture);
                    
                    table.AddCell(img);
                }
                else
                {
                    table.AddCell("");
                }
            }
            var table2 = new PdfPTable(2)
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                WidthPercentage = 50f,
                DefaultCell = { Padding = 10 }
            };
            //table2.SetWidths(new int[]{140,95});
            table2.AddCell(new Phrase("TOTAL AMOUNT", fntTableFontHdr));
            table2.AddCell(new Phrase(totalAmount.ToString("0.00") + "", fntTableFontHdr));

            document.Add(table);
            document.Add(table2);

            cb.EndText();
            document.Close();
            return null;
        }
    }
} 