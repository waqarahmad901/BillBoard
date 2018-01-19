using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        public ActionResult Index(string sortOrder, string archived = "", int page = 1, Guid? archive = null,int book = 1,string filter = "", string search2 = "", string search3 = "")
        {
            ViewBag.searchQuery = string.IsNullOrEmpty(filter) ? "" : filter;
            ViewBag.showArchived = (archived ?? "") == "on";

            page = page > 0 ? page : 1;
            int pageSize = 0;
            pageSize = pageSize > 0 ? pageSize : 100;
            ViewBag.search1 = filter;
            ViewBag.search2 = search2;
            ViewBag.search3 = search3;
            ViewBag.CurrentSort = sortOrder;

            IEnumerable<Customer> customers;
            var repository = new Repository<Customer>();
            
            customers = repository.GetAll().Where(x => 
            (string.IsNullOrEmpty(filter) || x.Catagory == null || x.Catagory.ToLower().Contains(filter.ToLower()))
            && (string.IsNullOrEmpty(search2) || x.Near == null || x.Near.ToLower().Contains(search2.ToLower()))
            && (string.IsNullOrEmpty(search3) || x.Brand == null || x.Brand.ToLower().Contains(search3.ToLower()))
         
            ); 
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
          
           

            return View(customers.ToPagedList(1, 100000));
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
                    if(sheet.Dimension == null)
                        return View();
                    string path = Path.Combine(ConfigurationManager.AppSettings["ImagePath"],"Book "+book);
                    List<Customer> customers = new List<Customer>();
                    var rowCnt = sheet.Dimension.End.Row;
                    for (int row = 2; row <= rowCnt; row++)
                    {
                        Customer customer = new Customer();
                        customer.RowGuid = Guid.NewGuid();
                        customer.SrNo = GetValue(sheet,row,1);
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
                        customer.BookNumber = GetIntValue(sheet, row, 16);
                        customer.Catagory = GetValue(sheet, row, 17) ;
                        //  customer.Picture = ConvertImageToBytes(Path.Combine(path , customer.SrNo + ""));
                        customer.CreatedAt = DateTime.Now;
                       // if(!string.IsNullOrEmpty(customer.Description) && !string.IsNullOrEmpty(customer.Location) && !string.IsNullOrEmpty(customer.Near))
                        customers.Add(customer);

                        if (customers.Count >= 1000)
                        {
                            repository.PostAll(customers);
                            customers.Clear();
                        }
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
            ViewBag.typesdd = repository.GetAll().GroupBy(x => x.Type).Select(x =>
           new SelectListItem { Text = x.First().Type, Value = x.First().Type }).ToList();

            var typefloatrepository = new Repository<lk_publicity_float>();
            ViewBag.typefloatdd = typefloatrepository.GetAll().Select(x =>
          new SelectListItem { Text = x.Catagory, Value = x.Catagory }).ToList();

            ViewBag.branddd = repository.GetAll().GroupBy(x => x.Brand).Select(x =>
          new SelectListItem { Text = x.First().Brand, Value = x.First().Brand }).ToList();
            var billAppender = new Repository<lk_BillAppender>();
            ViewBag.catdd = billAppender.GetAll().Select(x =>
       new SelectListItem { Text = x.Catagory, Value = x.Catagory }).ToList();
            return View(customer);
        }
        public ActionResult Delete(Guid? id,string brand)
        {
            var repository = new Repository<Customer>();
            Customer customer = repository.FindAll(x => x.RowGuid == id).FirstOrDefault();
            repository.Delete(customer.Id);
            return RedirectToAction("Detail",new { brand = brand });
        }
        [HttpPost]
        public ActionResult Edit(Customer customer, HttpPostedFileBase file)
        {
            var repository = new Repository<Customer>();
            Customer oCustomer = repository.Get(customer.Id);
           
             
            if (oCustomer == null)
                oCustomer = new Customer();
            oCustomer.RowGuid = Guid.NewGuid();
            oCustomer.SrNo = customer.SrNo;
            oCustomer.Description = customer.Description;
            oCustomer.Location = customer.Location;
            oCustomer.Near = customer.Near;
            if(!string.IsNullOrEmpty(customer.NewType))
                oCustomer.Type = customer.NewType;
            else
             oCustomer.Type = customer.Type;
            oCustomer.Size1 = customer.Size1;
            oCustomer.Size2 = customer.Size2;
            oCustomer.Size3 = customer.Size3;
            oCustomer.Size4 = customer.Size4;
            oCustomer.TotalMeasurment =(float.Parse(customer.Size1) * float.Parse(customer.Size2) * float.Parse(customer.Size3) * float.Parse(customer.Size4)).ToString();
            if (!string.IsNullOrEmpty(customer.NewBrand))
                oCustomer.Brand = customer.NewBrand;
            else
                oCustomer.Brand = customer.Brand;
            oCustomer.SurveyDate = customer.SurveyDate;
            oCustomer.BookNumber = customer.BookNumber;
            oCustomer.Catagory = customer.Catagory;
            if (customer.Type.ToLower() == "publicity float")
            {
                oCustomer.PublicityFloatCatagory = customer.PublicityFloatCatagory;
            }
            if (file != null)
            {
                string fileName = "~/Images/" + oCustomer.BookNumber + "/" + oCustomer.SrNo + System.IO.Path.GetExtension(file.FileName);
                string filePath = Server.MapPath(fileName);
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                file.SaveAs(filePath);
                
            }
            if (customer.Id == 0)
            {
                repository.Post(oCustomer);
            }
            else
            {

                repository.Put(oCustomer.Id,oCustomer);
            }
            return RedirectToAction("Index");
        }

        public byte[] ConvertImageToBytes(string path)
        {
           
            if (System.IO.File.Exists(path))
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
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

        

            billid = brandBill?.Id ?? 0;
            bill obill = null;
            if (billid > 0)
            {
                obill = new Repository<bill>().Get(billid);
                var billCustomers = obill.CustomerNames.Split(',');
                customers = customers.Where(x => x.Brand == obill.Brand).ToList();
                 customerDetailModels = customers.GroupBy(x => x.Description.Trim()).Select(x => new CustomerDetailModel
                {
                    CustomerName = x.Key.Trim(),
                    Selected = billCustomers.Contains(x.Key.Trim()), 
                    Customers = x.ToList()
                }).OrderByDescending(x=>x.Selected).ToList();

            }
            else
            {
                var catagoryRates = new Repository<lk_catagory_rates>();
                var allratesCatagory = catagoryRates.GetAll();

                var rates = new Repository<lk_rates>();
                var allrates = rates.GetAll(); 
                customers = customers.Where(x => x.Brand == brand).ToList();
                foreach (var item in customers)
                {
                    if (item.Type == "Publicity Float")
                    {
                        
                        var publicityfloatrate = new Repository<lk_publicity_float>();
                        var pCatagory = publicityfloatrate.GetAll().Where(x => x.Catagory == item.PublicityFloatCatagory).FirstOrDefault();
                        if (pCatagory != null)
                        {
                            item.FloatRate = pCatagory.PerDay;
                        }
                    }
                    else
                    {
                        string catagor = allratesCatagory.Where(x => x.Road == item.Location).Select(x => x.Catagory).FirstOrDefault();
                        catagor = catagor == null ? "A+" : catagor;

                        long perAnumRate = (long)(allrates.Where(x => x.Type == item.Type && x.Category == catagor).Select(x => x.Rate).FirstOrDefault());

                        item.Rates = perAnumRate;
                    }
                }
                customerDetailModels = customers.GroupBy(x => x.Description.Trim()).Select(x => new CustomerDetailModel
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
                detailList.BillPath = obill.FilePath;
                detailList.AmmementBillPath = obill.AmmendentBill;
                detailList.discountamountpaid = obill.Discount??0;
            }
            else
            {
                detailList.NumberMonth = 12;
                detailList.BillDate = DateTime.Now;
                
            }
            detailList.Comments = new Repository<Comment>().GetAll().Where(x => x.Brand.Trim() == brand.Trim()).ToList();
            detailList.Billid = obill == null ? "0" : obill.BillId;
            return View(detailList);
        }


        public ActionResult GetFloatRate(int id, string val)
        {
            var customerFloatCata = new Repository<Customer>().Get(id).PublicityFloatCatagory;
            if(customerFloatCata == null)
                return Json("0.00", JsonRequestBehavior.AllowGet);

            var publicityfloatrate = new Repository<lk_publicity_float>().GetAll().Where(x=>x.Catagory == customerFloatCata).FirstOrDefault();
            if (val == "Per day")
                return Json(publicityfloatrate.PerDay, JsonRequestBehavior.AllowGet);
            if (val == "Per week")
                return Json(publicityfloatrate.Perweek, JsonRequestBehavior.AllowGet);
            if (val == "Per 2 Week")
                return Json(publicityfloatrate.PerTwoWeek, JsonRequestBehavior.AllowGet);
            if (val == "Per Month")
                return Json(publicityfloatrate.PerMonth, JsonRequestBehavior.AllowGet);
            return Json("0.00", JsonRequestBehavior.AllowGet);

        }




        [HttpPost]
        public ActionResult SubmitDetail(CstomerDetilPageList details ,FormCollection collection)
        {
            var customerList = details.CustomerDetailList.Where(x => x.Selected).Select(x => x.CustomerName.Trim()).ToList();
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
                bill.Discount = details.discountamountpaid;
                repobill.Put(bill.Id, bill);
                return RedirectToAction("Index");
            }

            var catagoryRates = new Repository<lk_catagory_rates>();
            var allratesCatagory = catagoryRates.GetAll();

            List<Customer> customers = repository.GetAll().Where(x => customerList.Contains(x.Description.Trim()) && x.Brand.Trim() == details.Brand.Trim()).ToList();


            string[] keys =   collection.AllKeys.Where(x => x.StartsWith("rate_")).ToArray();
            foreach (var item in keys)
            {
                int custId = int.Parse(item.Split('_')[1]);
                if (!string.IsNullOrEmpty(Request[item]))
                {
                    decimal rate = decimal.Parse(Request[item]);
                    var cust = customers.Where(x => x.Id == custId).FirstOrDefault();
                    if(cust != null)
                    cust.Rates = rate;
                }
            }
             
             keys = collection.AllKeys.Where(x => x.StartsWith("floatfre_")).ToArray();
            foreach (var item in keys)
            {
                int custId = int.Parse(item.Split('_')[1]);
                if (!string.IsNullOrEmpty(Request[item]))
                {
                    var cust = customers.Where(x => x.Id == custId).FirstOrDefault();
                    if(cust != null)
                        cust.BillFrequency = Request[item];
                }
            }
            
           keys = collection.AllKeys.Where(x => x.StartsWith("floatrate_")).ToArray();
            foreach (var item in keys)
            {
                int custId = int.Parse(item.Split('_')[1]);
                if (!string.IsNullOrEmpty(Request[item]))
                {
                    decimal rate = decimal.Parse(Request[item]);
                    var cust = customers.Where(x => x.Id == custId).FirstOrDefault();
                    if(cust != null)
                        cust.FloatRate = rate;
                }
            }

            keys = collection.AllKeys.Where(x => x.StartsWith("floatmonth_")).ToArray();
            foreach (var item in keys)
            {
                int custId = int.Parse(item.Split('_')[1]);
                if (!string.IsNullOrEmpty(Request[item]))
                {
                    int month = int.Parse(Request[item]);
                    var cust = customers.Where(x => x.Id == custId).FirstOrDefault();
                    if(cust != null)
                        cust.FloatNumberMonth = month;
                }
            }




            repository.SaveChanges();
            var repoBill = new Repository<bill>();
            bill obill = null;
             
                obill = repoBill.GetAll().FirstOrDefault(x => x.Brand == details.Brand);
               if(obill == null) { 
                var rnd = new Random();
                var num = rnd.Next(0000000, 9999999);
                obill = new bill { FilePath = "", BillId = repoBill.GetAll().Count() == 0 ? "101" : (repoBill.GetAll().Select( x => int.Parse(x.BillId)).Max() + 1).ToString()};
                obill.BillDate = DateTime.Now;

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
            if(details.Billid != "0")
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
            obill.Discount = details.discountamountpaid;

            var comments = Request["txtcomments"];
            button = Request.Form["comment"];
            if (button != null )
            {
                if (!string.IsNullOrEmpty(comments))
                {
                    var repoComments = new Repository<Comment>();
                    var userId = (Session["user"] as ContextUser).OUser.Id;
                    Comment comm = new Comment { Brand = details.Brand, Comments = comments, CreatedAt = DateTime.Now, UserId = userId };
                    repoComments.Post(comm);
                }
                return RedirectToAction("Detail", new { brand = details.Brand });
            }

            var billAppernders = new Repository<lk_BillAppender>().GetAll(); 
            var catagory = customers.Select(x => x.Catagory).FirstOrDefault();
            catagory = string.IsNullOrEmpty(catagory) ? "Default" : catagory;
            string billApp = billAppernders.Where(x => x.Catagory.ToLower() == catagory.ToLower()).Select(x => x.BillNumberAppender).FirstOrDefault() + " ";

            string ammementButton = Request.Form["ammement"];
            List<PdfCoordinatesModel> pdfCoordinates = new List<PdfCoordinatesModel>()
            {
                new PdfCoordinatesModel {Text = obill.BillId, X = 117, Y = 831 ,IsBold = true},
                new PdfCoordinatesModel {Text = billApp, X = 160, Y = 830 ,IsBold = false,FontSize=14},
                new PdfCoordinatesModel {Text =   details.BillDate.ToString("dd/MM/yyyy"), X = 425, Y = 831,IsBold = true },
                new PdfCoordinatesModel {Text = customers.First().Brand, X = 264, Y = 806,IsBold = true},
                  new PdfCoordinatesModel { Type="amount", Text =  "", X = 427, Y = 590 ,IsBold = true},
            new PdfCoordinatesModel {Type="address", Text = "", X = 88, Y = 781 ,IsBold = true}
        }; 
            if (ammementButton != null)
            {
                pdfCoordinates.Add(new PdfCoordinatesModel { Text = "(AMENDED BILL)", X = 260, Y = 709, IsBold = true });
            }

            string imageFolderPath = Server.MapPath("~/Images");

            string filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
            string destinationFile = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile1 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile2 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            string destinationFile3 = Server.MapPath(Path.Combine(Path.GetDirectoryName(filePath), Guid.NewGuid() + ".pdf"));
            var totalamount = PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress,imageFolderPath, billApp,obill.Discount ?? 0);

            pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "/-";
            pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress + "";

            string aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates);
            MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile);

            if (!string.IsNullOrEmpty(details.BrandAddress1))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                 obill.BillId, "", ammementButton != null, details, details.BrandAddress1, imageFolderPath, billApp, obill.Discount ?? 0);
                pdfCoordinates.Where(x=>x.Type == "amount").First().Text = totalamount + "/-";
                pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress1 + "";

                aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates); 
                MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile1);
                 
            }
            if (!string.IsNullOrEmpty(details.BrandAddress2))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress2, imageFolderPath, billApp, obill.Discount ?? 0);
                pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "/-";
                pdfCoordinates.Where(x => x.Type == "address").First().Text = details.BrandAddress2 + "";
                aggrementfile = PdfGeneratorAggrement.GenerateOnflyPdf(Server.MapPath("~/Uploads/Bill/BillAggrementTemplate.pdf"), pdfCoordinates);
                  MergePDFs(new List<string> { Server.MapPath(filePath), aggrementfile }, destinationFile2);
               
            }
            if (!string.IsNullOrEmpty(details.BrandAddress3))
            {
                filePath = Path.Combine("~/Uploads", Guid.NewGuid()  + ".pdf");
                PdfGenerator.GenerateOnflyPdf(Server.MapPath(filePath), customers, allrates, allratesCatagory,
                obill.BillId, "", ammementButton != null, details, details.BrandAddress3, imageFolderPath, billApp, obill.Discount ?? 0);
                pdfCoordinates.Where(x => x.Type == "amount").First().Text = totalamount + "/-";
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

            return RedirectToAction("Detail", new { brand = details.Brand });
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
        public ActionResult BillManagement( string search2, string search3)
        {
            IEnumerable<bill> bills; 
            ViewBag.search2 = search2;
            ViewBag.search3 = search3; 

            var repository = new Repository<bill>();
            bills = repository.GetAll().Where(x =>
            (string.IsNullOrEmpty(search2) || x.Brand == null || x.Brand.ToLower().Contains(search2.ToLower()))
            && (string.IsNullOrEmpty(search3) || x.CustomerNames == null || x.CustomerNames.ToLower().Contains(search3.ToLower()))
         );
            return View(bills);
        }

        [HttpGet]
        public ActionResult BillReport(string filter, string search2,string search3)
        {
            ViewBag.search1 = filter;
            ViewBag.search2 = search2;
            ViewBag.search3 = search3;
            var date1 = string.IsNullOrEmpty(search2) ? DateTime.MinValue : DateTime.ParseExact(search2,"dd/MM/yyyy", CultureInfo.InvariantCulture);
            var date2 = string.IsNullOrEmpty(search3) ? DateTime.MinValue : DateTime.ParseExact(search3, "dd/MM/yyyy", CultureInfo.InvariantCulture);
           
            IEnumerable<bill> bills;
            var repository = new Repository<bill>();
            bills = repository.GetAll().Where(x=> 
                (string.IsNullOrEmpty(filter) || x.Brand.ToLower().Contains(filter.ToLower())) &&
                (date1 == DateTime.MinValue || x.CreatedAt >= date1) &&
                (date2 == DateTime.MinValue || x.CreatedAt <= date2.AddDays(1)) 
                ).ToList();
            return View(bills);
        }
    }
}