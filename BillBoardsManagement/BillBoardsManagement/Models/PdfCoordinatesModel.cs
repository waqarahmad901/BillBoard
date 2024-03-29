﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillBoardsManagement.Models
{
    public class PdfCoordinatesModel
    {
        public string Text { get; set; }
        public float X { get; set; }
        public float Y { get; set; } 
        public int Alignment { get; set; }
        public string Type { get; set; }
        public bool IsBold { get; set; }

        public int FontSize { get; set; } = 12;
    }
}