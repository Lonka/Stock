using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stock
{
    internal class StockItem
    {
        public StockItem(string code)
        {
            Code = code;
            AlarmLess = null;
            AlarmMore = null;
            AlarmMoreTime = DateTime.Now;
            AlarmLessTime = DateTime.Now;
            AlarmMoreMax = 5;
            AlarmLessMax = 5;
        }
        public string Code { get; set; }
        public double? AlarmMore { get; set; }

        public double? AlarmLess { get; set; }

        public DateTime AlarmMoreTime { get; set; }

        public int AlarmMoreMax { get; set; }

        public DateTime AlarmLessTime { get; set; }

        public int AlarmLessMax { get; set; }

    }

}
