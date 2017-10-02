﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using BillBoardsManagement.Models;
using BillBoardsManagement.Repository;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Font = iTextSharp.text.Font;

namespace BillBoardsManagement.Common
{
    public class PdfGenerator
    {
        public static decimal GenerateOnflyPdf(string filePath, IEnumerable<Customer> customers, IEnumerable<lk_rates> allrates, IEnumerable<lk_BillAppender> billAppender, IEnumerable<lk_catagory_rates> ratesCatagory,string billno, string billDate,bool isAmentment, CstomerDetilPageList brand,string address,string imagePath)
        {

            string oldFile = filePath;

            FileStream fs = new FileStream(filePath, FileMode.Create);

            Document document = new Document(PageSize.LEGAL, 25, 25, 30, 30);
            // Create an instance to the PDF file by creating an instance of the PDF 
            // Writer class using the document and the filestrem in the constructor.
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            document.Open();
          //  PdfContentByte cb = writer.DirectContent;
           // cb.BeginText();
            BaseFont fCb = BaseFont.CreateFont("c:\\windows\\fonts\\calibrib.ttf", BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
           // cb.SetFontAndSize(fCb, 9);
            var headerFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLACK);
            Paragraph header = new Paragraph("PARKS & HORTICULTURE AUTHORITY RAWALPINDI.", headerFont) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph1 = new Paragraph("RAJA BABAR LATIF CONTRACTOR ADVERTISEMENT FEE 2017 - 2018.", FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph2 = new Paragraph("RAWAL TOWN AREA.", FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph3 = new Paragraph("BILL.", FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLACK)) { Alignment = Element.ALIGN_CENTER };
            Paragraph paragraph4 = new Paragraph(customers.First().Description, FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK)) { Alignment = Element.ALIGN_LEFT };

            var billTable = new PdfPTable(2) 
            {
                WidthPercentage = 100,
                SpacingBefore = 5,
                DefaultCell = { Padding = 5,Border = 0 }
            };
            var catagory = customers.Select(x => x.Catagory).FirstOrDefault();
            string billApp = billAppender.Where(x => x.Catagory.ToLower() == catagory.ToLower()).Select(x=>x.BillNumberAppender).FirstOrDefault() + " ";
            billTable.AddCell(new PdfPCell(new Phrase("Bill No. " + billno + billApp, FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT,Border = iTextSharp.text.Rectangle.NO_BORDER});
            billTable.AddCell(new PdfPCell(new Phrase("Bill Date. " + brand.BillDate.ToString("dd/MM/yyyy"), FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });
             
            document.Add(header);
            document.Add(paragraph1);
            document.Add(paragraph2);
            document.Add(paragraph3);
            document.Add(paragraph4);
            document.Add(billTable); 
           
            if (isAmentment)
            {
                Paragraph paragraph7 = new Paragraph("AMENDED BILL", FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK)) {Alignment = Element.ALIGN_RIGHT};
                document.Add(paragraph7);
            }
            var fntTableFontHdr = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK);
            var fntTableFontRow = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK);
            var table = new PdfPTable(15)
            {
                WidthPercentage = 100,
                SpacingBefore = 20 ,
                DefaultCell = { Padding = 5}
            };
            
            table.SetWidths(new int[] { 60, 150, 150, 80, 65,35, 65, 35, 65, 35, 65, 70, 70, 90, 150 });
            table.AddCell(new PdfPCell(new Phrase("SR NO.", fntTableFontHdr)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("LOCATION", fntTableFontHdr)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("NEAR", fntTableFontHdr)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("TYPE OF ADVERTISEMENT", fntTableFontHdr)) { Rotation = 90, FixedHeight = 100, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, PaddingBottom = 5 });
            PdfPCell cell = new PdfPCell(new Paragraph("MEASURMENT", fntTableFontHdr) { Alignment = Element.ALIGN_CENTER }) { Colspan = 7, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
            table.AddCell(cell);
            table.AddCell(new PdfPCell(new Phrase("TOTAL MEASURMENT", fntTableFontHdr)) { Rotation = 90, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, PaddingBottom = 5 });

            table.AddCell(new PdfPCell(new Phrase("RATE PER SQ.FT PER ANUM", fntTableFontHdr)) { Rotation = 90, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("AMOUNT", fntTableFontHdr)) { Rotation = 90, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER });

            table.AddCell(new PdfPCell(new Phrase("IMAGE", fntTableFontHdr)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            int row = 1;
            decimal totalAmount = 0;

            foreach (var item in customers)
            {
               

                table.AddCell(new PdfPCell(new Phrase(row++ + "", fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(item.Location, fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(item.Near, fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(item.Type, fntTableFontRow)){ HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(removedec(item.Size1), fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase("X", fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(removedec(item.Size2), fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase("X", fntTableFontRow)){ HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(removedec(item.Size3), fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase("X", fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                table.AddCell(new PdfPCell(new Phrase(removedec(item.Size4), fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });


                //int string_length = item.TotalMeasurment.IndexOf('0');
                //string [] str_temp = item.TotalMeasurment.Split('.');
                //string val;
                //if (str_temp[1].Contains("0"))
                //{
                //    val = str_temp[0];
                //}
                //else
                //    val = item.TotalMeasurment;
                table.AddCell(new PdfPCell(new Phrase(removedec(item.TotalMeasurment), fntTableFontRow)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

                string catagor = ratesCatagory.Where(x => x.Road == item.Location).Select(x => x.Catagory).FirstOrDefault();
                catagor = catagor == null ? "A+" : catagor;
                long perAnumRate = 0;
                if (item.Rates == null)
                    perAnumRate = (long)(allrates.Where(x => x.Type == item.Type && x.Category == catagor && x.Brand == brand.IsBrand).Select(x => x.Rate).FirstOrDefault() * brand.NumberMonth);
                else
                    perAnumRate = (long)(item.Rates.Value * brand.NumberMonth);
                table.AddCell(new PdfPCell(new Phrase(perAnumRate + "", fntTableFontRow)){ HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                long amount =(long) (perAnumRate * decimal.Parse(item.TotalMeasurment));
                table.AddCell(new PdfPCell(new Phrase(amount.ToString("0") + "", fntTableFontRow)){ HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                totalAmount += amount;
                string filepath = Path.Combine(imagePath, item.BookNumber + "/" + item.SrNo + ".jpg");
                if (File.Exists(filepath))
                {
                    var img = iTextSharp.text.Image.GetInstance(filepath);

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
            table2.SetWidths(new int[]{140,95});
            table2.AddCell(new Phrase("TOTAL AMOUNT", fntTableFontHdr));
            table2.AddCell(new Phrase("Rs. "+totalAmount.ToString("0") + "/-", fntTableFontHdr));
            table.Complete = true;
            document.Add(table);
            document.Add(table2);

            Paragraph addressParagraph = new Paragraph(address, FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK)) { Alignment = Element.ALIGN_LEFT};

           // document.Add(addressParagraph);
         //   cb.EndText();
            document.Close();
            PageNumbering(filePath);
             

            return totalAmount;
        }

        private static string removedec (string str)
        {
            //            int string_length = str.IndexOf('0');

            if (str.Contains('.'))
            {
                string[] str_temp = str.Split('.');
                string val;
                if (str_temp[1].Contains("0") || str_temp[1].Contains("00"))
                {
                    val = str_temp[0];
                }
                else
                    val = str;

                return val;
            }
            return str;
        }

        private static void PageNumbering(string filePath)
        {
            int numbers = GetNumberOfPages(filePath);
            byte[] bytesfile = System.IO.File.ReadAllBytes(filePath);
            PdfReader reader = new PdfReader(filePath);

            byte[] bytes = null;
            using (var ms = new MemoryStream(bytesfile.Length))
            {
                using (PdfStamper stamper = new PdfStamper(reader, ms))
                {
                    for (int i = 1; i <= numbers; i++)
                    {
                        PdfContentByte canvas = stamper.GetOverContent(i);

                        ColumnText.ShowTextAligned(canvas, 0, new Phrase(i + " / " + numbers, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)), 570, 27, 0);
                    }
                }
                bytes = ms.ToArray();
            }
            reader.Close();
            File.WriteAllBytes(filePath, bytes);

        }

        public static int GetNumberOfPages(string path)
        {
            PdfReader pdfReader = new PdfReader(path);
            int numberOfPages = pdfReader.NumberOfPages;
            pdfReader.Close();
            return numberOfPages;
        }

    
    }
} 