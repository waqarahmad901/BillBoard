using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BillBoardsManagement.Common;
using BillBoardsManagement.Models;
using BillBoardsManagement.Repository;
using Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using LinqToExcel;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using PagedList;
using TmsWebApp.Common;
using Path = System.IO.Path;

namespace BillBoardsManagement.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index(string sortOrder, string filter = "", string archived = "", int page = 1, Guid? archive = null,int book = 1)
        {
            ViewBag.searchQuery = string.IsNullOrEmpty(filter) ? "" : filter;
            ViewBag.showArchived = (archived ?? "") == "on";

            page = page > 0 ? page : 1;
            int pageSize = 0;
            pageSize = pageSize > 0 ? pageSize : 100;

            ViewBag.CurrentSort = sortOrder;

            IEnumerable<Customer> customers;
            var repository = new Repository<Customer>();
            
            customers = repository.GetAll().Where(x => filter != null && x.Brand.ToLower().Contains(filter.ToLower()));; 
            //if (string.IsNullOrEmpty(filter))
            //{
            //    customers = repository.GetAll();
            //}
            //else
            //{
            //    customers = repository.GetAll(i => i,
            //        x => x.Brand.ToLower().Contains(filter.ToLower()) && x.BookNumber == book,
            //        i => i.Brand, false, null);
            //}


            customers = from x in customers
                        group x by x.Brand.Trim() into grp
                select grp.First();

            //   customers = customers.GroupBy(x => x.Brand).Select(x => x.First());

            //var books = repository.GetAll().GroupBy(x=>x.Brand).Select(x=>x.First()).Select(x =>
            // new SelectListItem { Text = "Book no "+ x.BookNumber , Value = x.BookNumber + "",Selected = x.BookNumber == book }).Distinct().ToList();

            //ViewBag.booksdd = books;
            ViewBag.bills = new Repository<bill>().GetAll();
            //Sorting order
            customers = customers.OrderBy(x => x.Brand);
            ViewBag.Count = customers.Count();
          
           

            return View(customers.ToPagedList(page, pageSize));
        }

       

        public ActionResult UploadExcel()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase file)
        {
            string fileName = "~/Uploads/" + file.FileName;
            string filePath = Server.MapPath(fileName);
            file.SaveAs(filePath);
           
            var repository = new Repository<Customer>();
            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(filePath)))
            {
                for (int book= 1; book <= xlPackage.Workbook.Worksheets.Count; book++)
                {
                    var sheet = xlPackage.Workbook.Worksheets[book];
                    string path = Path.Combine(ConfigurationManager.AppSettings["ImagePath"],"Book "+book);
                    List<Customer> customers = new List<Customer>();
                    var rowCnt = sheet.Dimension.End.Row;
                    for (int row = 2; row <= rowCnt; row++)
                    {
                        Customer customer = new Customer();
                        customer.RowGuid = Guid.NewGuid();
                        customer.SrNo = GetIntValue(sheet,row,1);
                        customer.Description =GetValue(sheet,row,2).ToUpper();
                        customer.Location = GetValue(sheet,row,3).ToUpper();
                        customer.Near = GetValue(sheet,row,4).ToUpper();
                        customer.Type = GetValue(sheet,row,5).ToUpper();
                        customer.Size1 = GetFloatValue(sheet,row,6) + "";
                        customer.Size2 = GetFloatValue(sheet,row,8) + "";
                        customer.Size3 = GetFloatValue(sheet,row,10) + "";
                        customer.Size4 = GetFloatValue(sheet,row,12) + "";
                        customer.TotalMeasurment = GetFloatValue(sheet,row,13) + "";
                        customer.Brand = GetValue(sheet,row,14).ToUpper();
                        customer.SurveyDate = GetDateValue(sheet,row,15); 
                        customer.BookNumber = book;
                      //  customer.Picture = ConvertImageToBytes(Path.Combine(path , customer.SrNo + ""));
                        customer.CreatedAt = DateTime.Now;
                        if(!string.IsNullOrEmpty(customer.Description) && !string.IsNullOrEmpty(customer.Location) && !string.IsNullOrEmpty(customer.Near))
                        customers.Add(customer);
                    }
                    repository.PostAll(customers);
                     
                } 
            } 
            return View();
        }

        public ActionResult Edit(Guid? id)
        {
            var repository = new Repository<Customer>();
            Customer customer = repository.FindAll(x => x.RowGuid == id).FirstOrDefault() ?? new Customer();
            return View(customer);
        }
        [HttpPost]
        public ActionResult Edit(Customer customer, HttpPostedFileBase file)
        {
            var repository = new Repository<Customer>();
            Customer oCustomer = repository.Get(customer.Id);
            if(oCustomer == null)
                oCustomer = new Customer();
            oCustomer.RowGuid = Guid.NewGuid();
            oCustomer.SrNo = customer.SrNo;
            oCustomer.Description = customer.Description;
            oCustomer.Location = customer.Location;
            oCustomer.Near = customer.Near;
            oCustomer.Type = customer.Type;
            oCustomer.Size1 = customer.Size1;
            oCustomer.Size2 = customer.Size2;
            oCustomer.Size3 = customer.Size3;
            oCustomer.Size4 = customer.Size4; 
            oCustomer.Brand = customer.Brand;
            oCustomer.SurveyDate = customer.SurveyDate;
            if (file != null)
            {
                string fileName = "~/Uploads/" + Path.GetFileNameWithoutExtension(file.FileName);
                string filePath = Server.MapPath(fileName);
                file.SaveAs(filePath);
                oCustomer.Picture = ConvertImageToBytes(filePath);
            }
            if (customer.Id == 0)
            {
                repository.Post(oCustomer);
            }
            else
            {

                repository.Put(oCustomer.Id,oCustomer);
            }
            return View(customer);
        }

        public byte[] ConvertImageToBytes(string path)
        {
            string imgePath = path + ".jpg";
            if (!System.IO.File.Exists(imgePath))
                imgePath = path + ".png";
            if (System.IO.File.Exists(imgePath))
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(imgePath))
                {
                    var resizedImage = resizeImage(image, new Size(500, 300));
                    using (MemoryStream m = new MemoryStream())
                    {
                        resizedImage.Save(m, ImageFormat.Bmp);
                        return m.ToArray();  
                    }
                }
            }
            return null;
        }

        public System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            return (System.Drawing.Image)(new Bitmap(imgToResize, size));
        }

        private string GetValue(ExcelWorksheet sheet, int row, int col)
        {
            if (sheet.Cells[row, col].Value == null)
            {
                return "";
            }
            return sheet.Cells[row, col].Value.ToString();
        }
        private DateTime? GetDateValue(ExcelWorksheet sheet, int row, int col)
        {
            if (sheet.Cells[row, col].Value == null)
            {
                return null;
            }
            string value = GetValue(sheet, row, col);
            DateTime date = DateTime.Now;
            string[] formats = {"M/d/yyyy", "MM/d/yyyy"};

            if (DateTime.TryParse(value,out date))
                return date;
            return null;
        }
        private float GetFloatValue(ExcelWorksheet sheet, int row, int col)
        {
            if (sheet.Cells[row, col].Value == null)
            {
                return 0f;
            }
            string value = GetValue(sheet, row, col);
            float result = 0;
            if (float.TryParse(value, out result))
                return result;
            return 0f;
        }
        private int GetIntValue(ExcelWorksheet sheet, int row, int col)
        {
            if (sheet.Cells[row, col].Value == null)
            {
                return 0;
            }
            string value = GetValue(sheet, row, col);
            int result = 0;
            if (int.TryParse(value, out result))
                return result;
            return 0;
        }


        public ActionResult Detail(string brand, string filter = "", string archived="", int page = 1, Guid? archive = null, int book = 1,int billid = 0)
        {
            ViewBag.searchQuery = string.IsNullOrEmpty(filter) ? "" : filter;
            ViewBag.showArchived = (archived ?? "") == "on";

            page = page > 0 ? page : 1;
            int pageSize = 0;
            pageSize = pageSize > 0 ? pageSize : 100; 
            IEnumerable<Customer> customers;
            var repository = new Repository<Customer>();
            customers = repository.GetAll();
           
            List<CustomerDetailModel> customerDetailModels = null;
            var brandBill = new Repository<bill>().GetAll().FirstOrDefault(x => x.Brand == brand);

            var lastRecord = new Repository<bill>().GetAll().Last();

            billid = brandBill?.Id ?? 0;
            bill obill = null;
            if (billid > 0)
            {
                obill = new Repository<bill>().Get(billid);
                var billCustomers = obill.CustomerNames.Split(',');
                customers = customers.Where(x => x.Brand == obill.Brand).ToList();
                 customerDetailModels = customers.GroupBy(x => x.Description).Select(x => new CustomerDetailModel
                {
                    CustomerName = x.Key.Trim(),
                    Selected = billCustomers.Contains(x.Key.Trim()),
                    
                    Customers = x.ToList()
                }).OrderByDescending(x=>x.Selected).ToList();

            }
            else
            {
                customers = customers.Where(x => x.Brand == brand).ToList();
                customerDetailModels = customers.GroupBy(x => x.Description).Select(x => new CustomerDetailModel
                {
                    Selected = true,
                    CustomerName = x.Key.Trim(),
                    Customers = x.ToList(),
                    
                }).ToList();
            }
            //var books = repository.GetAll().GroupBy(x=>x.Brand).Select(x=>x.First()).Select(x =>
            // new SelectListItem { Text = "Book no "+ x.BookNumber , Value = x.BookNumber + "",Selected = x.BookNumber == book }).Distinct().ToList();

            //ViewBag.booksdd = books;

            //Sorting order
            
            ViewBag.Count = customerDetailModels.Count();
            var detailList = new CstomerDetilPageList();
            detailList.CustomerDetailList = customerDetailModels;

            if (obill != null)
            {
                detailList.Brand = obill.Brand;
                detailList.Billid = obill.BillId;
                detailList.BrandAddress = obill.BrandAddress;
                detailList.NumberMonth = obill.NumberMonth ?? 0;
                detailList.TrakingNumber = obill.TrakingNumber;
                detailList.ShippingDate = obill.ShippingDate ?? DateTime.Now;
                detailList.BillDate = obill.BillDate ?? DateTime.Now;
                detailList.billamountpaid = obill.BillAmountPaid ?? 0;
                detailList.billamountgenerated = obill.BillAmountGenerated ?? 0;
                detailList.ContactPersonDesignation = obill.ContactPersonDesignation;
                detailList.ContactPersonDesignation1 = obill.ContactPersonDesignation1;
                detailList.ContactPersonMobile = obill.ContactPersonMobile;
                detailList.ContactPersonMobile1 = obill.ContactPersonMobile1;
                detailList.ContactPersonName = obill.ContactPersonName;
                detailList.ContactPersonName1 = obill.ContactPersonName1;
                detailList.BrandAddress1 = obill.BrandAddress1;
                detailList.BrandAddress2 = obill.BrandAddress2;
                detailList.BrandAddress3 = obill.BrandAddress3;
            }
            else
            {
                detailList.NumberMonth = 12;
                detailList.BillDate = DateTime.Now;
                
            }
            detailList.Billid = billid + "";
            return View(detailList);
        }
        [HttpPost]
        public ActionResult SubmitDetail(CstomerDetilPageList details )
        {
            var customerList = details.CustomerDetailList.Where(x => x.Selected).Select(x => x.CustomerName).ToList();
            var repository = new Repository<Customer>();
            var rates = new Repository<lk_rates>();
            var allrates = rates.GetAll();
            string button = Request.Form["updatebutton"];
            if (button != null)
            {
                var repobill = new Repository<bill>();
                var bill = repobill.GetAll().Where(x=>x.Brand == details.Brand).FirstOrDefault();
                bill.BrandAddress = details.BrandAddress;
                bill.ShippingDate = details.ShippingDate;
                bill.BillAmountPaid = details.billamountpaid;
                bill.BillDate = details.BillDate;
                bill.BillId = details.Billid;
                bill.BrandAddress1 = details.BrandAddress1;
                bill.BrandAddress2 = details.BrandAddress2;
                bill.BrandAddress3 = details.BrandAddress3;
                bill.ContactPersonDesignation = details.ContactPersonDesignation;
                bill.ContactPersonDesignation1 = details.ContactPersonDesignation1;
                bill.ContactPersonMobile = details.ContactPersonMobile;
                bill.ContactPersonMobile1 = details.ContactPersonMobile1;
                bill.ContactPersonName = details.ContactPersonName;
                bill.ContactPersonName1 = details.ContactPersonName1;
                repobill.Put(bill.Id, bill);
                return RedirectToAction("Index");
            }

            var catagoryRates = new Repository<lk_catagory_rates>();
            var allratesCatagory = catagoryRates.GetAll();

            IEnumerable<Customer> customers = repository.GetAll().Where(x => customerList.Contains(x.Description) && x.Brand.ToLower() == details.Brand.ToLower()).ToList();

           
            
            var repoBill = new Repository<bill>();
            bill obill = null;
             
                obill = repoBill.GetAll().FirstOrDefault(x => x.Brand == details.Brand);
               if(obill == null) { 
                var rnd = new Random();
                var num = rnd.Next(0000000, 9999999);
                obill = new bill { FilePath = "", BillId =  (repoBill.GetAll().Count() + 1).ToString().PadLeft(3,'0') };
                } 
          
            obill.Brand = details.Brand;
            obill.CustomerNames = string.Join(",", customerList); 
            obill.CreatedAt = DateTime.Now;
            obill.CreatedBy = 1;
            obill.BrandAddress = details.BrandAddress;
            obill.TrakingNumber = details.TrakingNumber;
            obill.NumberMonth = details.NumberMonth;
            if (DateTime.MinValue == details.ShippingDate) obill.ShippingDate = null;
            else obill.ShippingDate = details.ShippingDate;
            obill.BillAmountPaid = details.billamountpaid;
            obill.BillDate = details.BillDate;
            obill.BillId = details.Billid;
            obill.BrandAddress1 = details.BrandAddress1;
            obill.BrandAddress2 = details.BrandAddress2;
            obill.BrandAddress3 = details.BrandAddress3;
            obill.ContactPersonDesignation = details.ContactPersonDesignation;
            obill.ContactPersonDesignation1 = details.ContactPersonDesignation1;
            obill.ContactPersonMobile = details.ContactPersonMobile;
            obill.ContactPersonMobile1 = details.ContactPersonMobile1;
            obill.ContactPersonName = details.ContactPersonName;
            obill.ContactPersonName1 = details.ContactPersonName1;


            string ammementButton = Request.Form["ammement"];
            List<PdfCoordinatesModel> pdfCoordinates = new List<PdfCoordinatesModel>()
            {
                new PdfCoordinatesModel {Text = obill.BillId, X = 125, Y = 805 },
                new PdfCoordinatesModel {Text =   details.BillDate.ToString("MM/dd/yyyy"), X = 390, Y = 805 },
                new PdfCoordinatesModel {Text = obill.Brand, X = 264, Y = 782},
                  new PdfCoordinatesModel { Type="amount", Text =  "", X = 110, Y = 585 },
            new PdfCoordinatesModel {Type="address", Text = "", X = 88, Y = 761 }
        }; 
            if (ammementButton != null)
            {
                pdfCoordinates.Add(new PdfCoordinatesModel { Text = "Amendment", X = 150, Y = 712 });
            }


            string filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
            string destinationFile = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile1 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile2 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile3 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));

            var totalamount = PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress);

            pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "";
            pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress + "";

            string aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates);
            MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile);

            if (!string.IsNullOrEmpty(details.BrandAddress1))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                 obill.BillId, "", ammementButton != null, details, details.BrandAddress1);
                pdfCoordinates.Where(x=>x.Type == "amount").First().Text = totalamount + "";
                pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress1 + "";

                aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates); 
                MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile1);
                 
            }
            if (!string.IsNullOrEmpty(details.BrandAddress2))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress2);
                pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "";
                pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress2 + "";
                aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates);
                  MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile2);
               
            }
            if (!string.IsNullOrEmpty(details.BrandAddress3))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress3);
                pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "";
                pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress3 + ""; aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates);
                
                MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile3);
                 
            }
            string mergerdFile = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));

            MergePDFs(new List<string> { destinationFile, destinationFile1,destinationFile2,destinationFile3}, mergerdFile);

            obill.BillAmountGenerated = totalamount;

            if(ammementButton != null)
                obill.AmmendentBill = "~/Uploads/" + Path.GetFileName(mergerdFile);
            else
                obill.FilePath = "~/Uploads/" + Path.GetFileName(mergerdFile);

            if (obill.Id > 0)
            { 
                repoBill.Put(obill.Id, obill);
            }
            else
            { 
                repoBill.Post(obill);
            }
             
            return RedirectToAction("Index");
        }

        public static bool MergePDFs(IEnumerable<string> fileNames, string targetPdf)
        {
            bool merged = true;
            using (FileStream stream = new FileStream(targetPdf, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    foreach (string file in fileNames)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            reader = new PdfReader(file);
                            pdf.AddDocument(reader);
                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    merged = false;
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                finally
                {
                    if (document != null)
                    {
                        document.Close();
                    }
                }
            }
            return merged;
        }



        [HttpGet]
        public ActionResult BillManagement()
        {
            IEnumerable<bill> bills;
            var repository = new Repository<bill>();
            bills = repository.GetAll();
            return View(bills);
        }
    }
}