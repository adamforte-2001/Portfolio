using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Project1
{
    public class aSmartCandlestick : candleStick
    {
        /*
         Add the following properties to the base candlestick class 
         used in project 1:
         
         1: Top: top body price
         2: Bottom: bottom body price
         3: UpperTail: distance between high and top price
         4: LowerTail: distance between low and bottom price
         5: BodyRange: distance between top and bottom
         6: Range: high - low
         7: IsDoji: top is roughly bottom price
         8: IsDragonFly: doji and close to the High
         9: IsGravestone: doji and close to the Low 
         10: Hammer: body is close to the tope
         11: Bullish: price over the period goes up 
         12: Bearish: price ove the period goes down
         13: IsMarubozu: small tails


         
         
         */

        public double TopPrice {
            get { return Math.Max(this.Open, this.Close); }
        }

        public double BottomPrice {
            get { return Math.Min(this.Open, this.Close); }
        }

        public double UpperTail {
            get { return this.High - this.TopPrice; }
        }

        public double LowerTail {
            get { return this.BottomPrice - this.Low; }
        }

        public double BodyRange {
            get { return this.TopPrice - this.BottomPrice; }
        }

        public double Range {
            get { return this.High - this.Low; }
        }

        public bool IsDoji {
            get { return this.TopPrice - this.BottomPrice <= 0.5; }
        }

        public bool IsDragonfly {
            get { return this.IsDoji && (this.High - this.TopPrice <= 0.125 * this.Range); }
        }
        
        public bool IsGravestone {
            get { return this.IsDoji && (this.BottomPrice - this.Low <= 0.125 * this.Range); }
        }

        public bool IsMarubozu {
            get { return this.Range - this.BodyRange <= 0.03 * this.Range; }
        }

        public bool IsBullish {
            get { return this.Open < this.Close; }
        
        }

        public bool IsBearish {
            get { return this.Close < this.Open; }
        }

        public bool IsNeutral {
            get { return this.IsDoji;  }
        }

        public bool Hammer {
            get { return this.BodyRange <= 0.23 * this.Range && this.BodyRange >= 0.05 * this.Range && this.TopPrice > 0.9 * this.Range; }
        }
    }
}
