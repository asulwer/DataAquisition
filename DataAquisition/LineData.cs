using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PhoneApp1
{
    public class LineData
    {
        public string Category { get; set; }
        public decimal High { get; set; }
        public decimal Price { get; set; }
        public decimal Low { get; set; }
    }
    public class Feed
    {
        public Feed()
        {
            Symbol = "";
            Price = 0;
            Date = DateTime.Now;
            Change = 0;
            Open = 0;
            High = 0;
            Low = 0;
            Volume = 0;
        }

        public Feed(string symbol, Decimal price, DateTime date, Decimal change, Decimal open, Decimal high, Decimal low, Int64 volume)
        {
            Symbol = symbol;
            Price = price;
            Date = date;
            Change = change;
            Open = open;
            High = high;
            Low = low;
            Volume = volume;
        }

        //order by Date
        public static int CompareDate(Feed d1, Feed d2)
        {
            return d2.Date.CompareTo(d1.Date);
        }

        //order by volume
        public static int CompareVolume(Feed d1, Feed d2)
        {
            return d2.Volume.CompareTo(d1.Volume);
        }

        [Browsable(false)]
        public string Symbol { get; set; }
        public Decimal Price { get; set; }
        public DateTime Date { get; set; }
        public Decimal Change { get; set; }
        public Decimal Open { get; set; }
        public Decimal High { get; set; }
        public Decimal Low { get; set; }
        public Int64 Volume { get; set; }
    }
}
