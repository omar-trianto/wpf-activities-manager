using System;
using System.Collections.Generic;
using System.Text;

namespace activities_wpf.DataModel
{
    public class Activity
    {
        public int ActivityID { get; set; }
        //public string DateStartTime { get; set; } = "";
        public DateTime DateStartTime { get; set; }
        public string Title { get; set; } = "";

        public decimal Cost { get; set; }

        public string ActivityType { get; set; } = "";
    }
}
