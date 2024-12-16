using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Project1
{
    internal static class CandleStickLoader
    {
        //simple class to parse the csv files
        public static List<aSmartCandlestick> Load(String FilePath) {
            //returns a list of candlesticks
            List<aSmartCandlestick> ret = new List<aSmartCandlestick>();
            String[] lines = System.IO.File.ReadAllLines(FilePath);
            for (int i = 1; i < lines.Length; i++)
            {
                String[] attributes = lines[i].Split(',');
                ret.Add(
                    new aSmartCandlestick
                    {
                        Open = Double.Parse(attributes[1]),
                        High =  Double.Parse(attributes[2]),
                        Low = Double.Parse(attributes[3]),
                        Close = Double.Parse(attributes[4]),
                        Volume = Int64.Parse(attributes[5]),
                        Date = DateTime.Parse(attributes[0])
                    }
                );
            }
            return ret;
        }
    }
}
