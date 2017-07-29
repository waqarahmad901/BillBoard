using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BillBoardsManagement.Repository;
using Excel;
using LinqToExcel;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using PagedList;

namespace BillBoardsManagement.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index(string sortOrder, string filter, string archived, int page = 1, Guid? archive = null,int book = 1)
        {
            ViewBag.searchQuery = string.IsNullOrEmpty(filter) ? "" : filter;
            ViewBag.showArchived = (archived ?? "") == "on";

            page = page > 0 ? page : 1;
            int pageSize = 0;
            pageSize = pageSize > 0 ? pageSize : 100;

            ViewBag.CurrentSort = sortOrder;

            IEnumerable<Customer> customers;
            var repository = new Repository<Customer>();

            if (string.IsNullOrEmpty(filter))
            {
                customers = repository.GetAll(i => i, 
                    x => x.BookNumber == book,
                    i=>i.Brand,false,null);
            }
            else
            {
                customers = repository.GetAll(i => i,
                    x => x.Brand.ToLower().Contains(filter.ToLower()) && x.BookNumber == book,
                    i => i.Brand, false, null);
            }

            var books = repository.GetAll().GroupBy(x=>x.BookNumber).Select(x=>x.First()).Select(x =>
             new SelectListItem { Text = "Book no "+ x.BookNumber , Value = x.BookNumber + "",Selected = x.BookNumber == book }).Distinct().ToList();
                
            ViewBag.booksdd = books;

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
                    //var pic = sheet.Drawings["Picture 35"] as ExcelPicture;
                    //var path = Server.MapPath("~/Uploads/Picture 35.jpg");
                    //pic.Image.Save(path);
                    List<Customer> customers = new List<Customer>();
                    var rowCnt = sheet.Dimension.End.Row;
                    for (int row = 2; row <= rowCnt; row++)
                    {
                        Customer customer = new Customer();
                        customer.RowGuid = Guid.NewGuid();
                        customer.SrNo = GetIntValue(sheet,row,1);
                        customer.Description = GetValue(sheet,row,2);
                        customer.Location = GetValue(sheet,row,3);
                        customer.Near = GetValue(sheet,row,4);
                        customer.Type = GetValue(sheet,row,5);
                        customer.Size1 = GetFloatValue(sheet,row,6);
                        customer.Size2 = GetFloatValue(sheet,row,8);
                        customer.Size3 = GetFloatValue(sheet,row,10);
                        customer.Size4 = GetFloatValue(sheet,row,12);
                        customer.TotalMeasurment = GetFloatValue(sheet,row,13);
                        customer.Brand = GetValue(sheet,row,14);
                        customer.SurveyDate = GetDateValue(sheet,row,15); 
                        customer.BookNumber = book;
                        customer.CreatedAt = DateTime.Now;
                        if(!string.IsNullOrEmpty(customer.Description) && !string.IsNullOrEmpty(customer.Location) && !string.IsNullOrEmpty(customer.Near))
                        customers.Add(customer);
                    }
                    repository.PostAll(customers);

                  
                   
                } 
            }
             

            return View();
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




    }
}