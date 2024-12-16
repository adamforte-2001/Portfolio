using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;

namespace Project1
{
    public partial class Form_Input : Form
    {
        private List<aSmartCandlestick> unfiltered; // list to be filtered
        private BindingList<aSmartCandlestick> src; //filtered binding list

        /* fields for handling rubberbanding */
        private Point chartAreaTop;
        private Point chartAreaBottom;
        private bool selecting = false;
        private Point initial;
        private Rectangle selectionRectangle;

        private double TopAreaRange {
            get { return this.chart_cs.ChartAreas["ChartArea1"].AxisY.Maximum - this.chart_cs.ChartAreas["ChartArea1"].AxisY.Minimum; }
        }
        public Form_Input()
        {
            InitializeComponent();
            this.DateTimePicker_Start.Value = DateTime.Now.AddDays(-365);
            this.DateTimePicker_End.Value = DateTime.Now;
        }

        public Form_Input(string fileName, DateTime start, DateTime end) {
            //constructor to make a new form 
            InitializeComponent();
            this.DateTimePicker_Start.Value = start;
            this.DateTimePicker_End.Value = end;
            this.unfiltered = CandleStickLoader.Load(fileName);
            this.src = new BindingList<aSmartCandlestick>();
            //this.chart_cs = new Chart();
            filterCandlesticks();
            displayCandleSticks();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // unused, left here for future development
        }
        private void Button_LoadStocks_Click(object sender, EventArgs e)
        {
            //brings up the file picker dialoge to get csv data
            getCSV_openFileDialog.ShowDialog();
        }

        private void getCSV_openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            //triggers after usere selects a file. gets the csv data and parses it,
            //calls functions to filter data to the date range, and display in the data grid view and the chart
            this.unfiltered = CandleStickLoader.Load(getCSV_openFileDialog.FileName);
            this.src = new BindingList<aSmartCandlestick>();
            filterCandlesticks();
            displayCandleSticks();
            double area1x = this.chart_cs.ChartAreas["ChartArea1"].Position.X;
            double area1y = this.chart_cs.ChartAreas["ChartArea1"].Position.Y;
            double area1Height = this.chart_cs.ChartAreas["ChartArea1"].Position.Height;
            double area1Width = this.chart_cs.ChartAreas["ChartArea1"].Position.Width;

            int x = this.chart_cs.Location.X;
            int y = this.chart_cs.Location.Y;
            int height = this.chart_cs.Height;
            int width = this.chart_cs.Width;

            if (this.getCSV_openFileDialog.FileNames.Length > 1) {
                var names = this.getCSV_openFileDialog.FileNames;
                for (int i = 1; i < names.Length; i++) {
                    new Form_Input(names[i], this.DateTimePicker_Start.Value, this.DateTimePicker_End.Value).Show();
                }
            }

        }

        /*****************************
         *
         * Helper functions
         *
         ******************************/

        private void filterCandlesticks() {
            //performs the filter operation on the list of candlesticks gathered from the csv file. 
            //empties the binding list and refills it with data in the proper date range
            this.src.Clear();
            foreach (aSmartCandlestick stick in this.unfiltered) {
                if (stick.Date < this.DateTimePicker_Start.Value || stick.Date > this.DateTimePicker_End.Value) {
                    continue;
                }
                this.src.Add(stick);
                System.Console.WriteLine(stick);
            }
        }

        private void displayCandleSticks()
        {
            //binds the binding list to the dataGridView, and sets parameters for data entry into the chart. 
            //populates the chart with filtered candlestick and volume data

            //set apperence properties for the candlestick data, and get chart ready for new data
            chart_cs.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
            chart_cs.Annotations.Clear();
            chart_cs.Series["Series1"].Points.Clear();

            chart_cs.Series["Series1"]["PriceUpColor"] = "Green";
            chart_cs.Series["Series1"]["PriceDownColor"] = "Red";
            chart_cs.Series[0].XValueType = ChartValueType.DateTime;
            double max = src.Max(stick => stick.High);
            chart_cs.ChartAreas["ChartArea1"].AxisY.Maximum = max + 0.02 * max;
            chart_cs.ChartAreas["ChartArea1"].AxisY.Minimum = src.Min(stick => stick.Low) - 0.02 * max;

            foreach (var candle in src)
            {
                chart_cs.Series["Series1"].Points.AddXY(candle.Date.ToOADate(), candle.High, candle.Low, candle.Open, candle.Close);
            }
            markPeaksAndValleys();
            this.Refresh();
            var area = this.chart_cs.ChartAreas[0];
            foreach (var cs in src) {
                double x = area.AxisX.ValueToPixelPosition(cs.Date.ToOADate());
                double y = area.AxisY.ValueToPixelPosition(cs.Close);
            }
        }

        private double findMin(BindingList<aSmartCandlestick> src)
        {
            //finds the minimum numeric value in the binding list of candlesticks, used to normalize the data in the candlestick
            double minValue = double.MaxValue;

            // Iterate through each Candlestick
            foreach (var candle in src)
            {
                // Compare each property and update minValue if a smaller value is found
                minValue = Math.Min(minValue, candle.Open);
                minValue = Math.Min(minValue, candle.High);
                minValue = Math.Min(minValue, candle.Low);
                minValue = Math.Min(minValue, candle.Close);
            }

            return minValue;
        }

        private void markPeaksAndValleys()
        {

            for (int i = 1; i < this.src.Count - 1; i++)
            {

                if (this.src[i].High > this.src[i - 1].High && this.src[i].High > this.src[i + 1].High)
                {
                    this.src[i].IsPeak = true;
                    continue;
                }
                else if (this.src[i].Low < this.src[i - 1].Low && this.src[i].Low < this.src[i + 1].Low)
                {
                    this.src[i].IsValley = true;
                }
            }
        }
        private bool inCandlestickBounds(Point p)
        {
            return (p.X >= this.chartAreaTop.X && p.X <= this.chartAreaBottom.X &&
                    p.Y >= this.chartAreaTop.Y && p.Y <= this.chartAreaBottom.Y);
        }

        private void addAnnotation(bool peak, int index)
        {
            //create a text annotation and a stripline to mark peaks and valleys
            TextAnnotation textAnnotation = new TextAnnotation
            {
                Text = peak ? "Peak" : "Valley",
                AnchorDataPoint = chart_cs.Series["Series1"].Points[index],
                ForeColor = peak ? System.Drawing.Color.Green : System.Drawing.Color.Red,
                Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold)
            };

            StripLine horizontalLine = new StripLine
            {
                Interval = 0, // Ensures it's a single line
                IntervalOffset = peak ? this.src[index].High : this.src[index].Low,  // Y value for the line based of candlesticks and whether it's a peak or a valley
                StripWidth = 0.01,
                BackColor = peak ? System.Drawing.Color.Green : System.Drawing.Color.Red // Line color based on type of extreme
            };
            chart_cs.ChartAreas["ChartArea1"].AxisY.StripLines.Add(horizontalLine);

            chart_cs.Annotations.Add(textAnnotation);
        }

        private int getXFromDate(double date) {
            int areaxmin = this.chart_cs.Location.X + (int)(this.chart_cs.Width * (this.chart_cs.ChartAreas[0].Position.X / 100));
            int areaxmax = areaxmin + (int)((this.chart_cs.ChartAreas[0].Position.Width / 100) * this.chart_cs.Width);
            double min = chart_cs.ChartAreas[0].AxisX.Minimum;
            double max = chart_cs.ChartAreas[0].AxisX.Maximum;
            double proportion = (date - min) / (max - min);
            return (int)(areaxmin + proportion * (areaxmax - areaxmin));

        }

        private bool markWave() {
            var area = chart_cs.ChartAreas[0];
            chart_cs.Annotations.Clear();
            int first = 0;
            double limit = 0;
            int last = 1;

            for (int i = 1; i < src.Count; i++) {
                Point high = new Point((int)area.AxisX.ValueToPixelPosition(src[i].Date.ToOADate()), (int)(area.AxisY.ValueToPixelPosition(src[i].High)));
                Point low = new Point((int)area.AxisX.ValueToPixelPosition(src[i].Date.ToOADate()), (int)(area.AxisY.ValueToPixelPosition(src[i].Low)));
                if (first < 1 && selectionRectangle.Contains(high) && selectionRectangle.Contains(low)
                    && (src[i].IsPeak || src[i].IsValley))
                {
                    first = i;
                    last = i;
                    limit = src[i].IsPeak ? src[i].High : src[i].Low;
                }
                else if (first >= 1) {
                    if ((src[first].IsValley && src[i].Low < limit) || (!src[first].IsValley && src[i].High > src[first].High) ||
                        (!(selectionRectangle.Contains(high) && selectionRectangle.Contains(low)))) {
                        break;
                    }
                    last = i;
                }
            }
            if (first == 0 || first == last) {
                return false;
            }

            double reference = chart_cs.ChartAreas[0].AxisY.Maximum - chart_cs.ChartAreas[0].AxisY.Minimum;
            double h = reference * 0.05;
            double w = h / 3;
            double offset = reference * 0.015;
            ArrowAnnotation firstArrow = new ArrowAnnotation
            {
                LineColor = Color.Black,    // Color of the arrow
                LineWidth = 2,           // Thickness of the arrow
                ArrowStyle = ArrowStyle.Simple
            };
            firstArrow.AxisX = area.AxisX; // Detach from axes
            firstArrow.AxisY = area.AxisY;
            firstArrow.AnchorX = src[first].Date.ToOADate(); // Disable relative X
            firstArrow.AnchorY = src[first].IsPeak ? src[first].High + offset : src[first].Low - offset; 
            firstArrow.Width = 0;      // Arrow width (for direction)
            firstArrow.Height = src[first].IsPeak ? -h : h;   // Negative height for upside-down direction

            ArrowAnnotation secondArrow = new ArrowAnnotation
            {
                LineColor = Color.Black,    // Color of the arrow
                LineWidth = 2,           // Thickness of the arrow
                ArrowStyle = ArrowStyle.Simple
            };
            secondArrow.AxisX = area.AxisX; // Detach from axes
            secondArrow.AxisY = area.AxisY;
            secondArrow.AnchorX = src[last].Date.ToOADate(); // Disable relative X
            secondArrow.AnchorY = src[first].IsValley ? src[last].High + offset: src[last].Low - offset;// src[first].IsPeak ? src[first].High + offset : src[first].Low - offset;
            secondArrow.Width = 0;      // Arrow width (for direction)
            secondArrow.Height = src[first].IsPeak ? h : -h;   // Negative height for upside-down direction

            chart_cs.Annotations.Add(firstArrow);
            chart_cs.Annotations.Add(secondArrow);
            double price = src[first].IsPeak ? src.Skip(first).Take(last - first + 1).Min(cs => cs.Low) : src.Skip(first).Take(last - first + 1).Max(cs => cs.High);
            getRangeBeauty(first, last, price, true);
            
            double initialPrice = price;
            chart_cs.Series[1].Points.Clear();
            while (src[first].IsPeak ? price >= 0.75 * initialPrice : price <= 1.25 * initialPrice)
            {
                chart_cs.Series[1].Points.AddXY(price, getRangeBeauty(first, last, price));
                price = (src[first].IsPeak ? price - 0.005 * initialPrice : price + 0.005 * initialPrice);
            }
            
            return true;

        }
        private double getRangeBeauty(int low, int high, double price, bool addAnnotations = false) {
            double rangeHigh, rangeLow;

            if (src[low].IsPeak)
            {
                rangeHigh = src[low].High;
                rangeLow = price;
            }
            else {
                rangeLow = src[low].Low;
                rangeHigh = price;
            }
            
            double[] fibFractions = { 0, 0.236, 0.382, 0.5, 0.628, 0.764, 1 };
            var fibLevels = fibFractions.Select(f => rangeLow + (rangeHigh - rangeLow) * f);

            if (addAnnotations) redrawLines(fibLevels.ToArray<double>());

            double offset = 0.02 * (rangeHigh - rangeLow);
            double total = 0;
            for (int i = low; i <= high; i++) {
                foreach (double level in fibLevels) {
                    double[] tests = { src[i].Low, src[i].BottomPrice, src[i].TopPrice, src[i].High };
                    foreach (double value in tests)
                    {
                        if (value >= level - offset && value <= level + offset)
                        {
                            total += 2.5;
                            if (addAnnotations) addDot(src[i].Date.ToOADate(),value);
                        }
                    }
                }
            }

            return total / (high - low + 1);
        }

        private void addDot(double x, double y) {
            var addition = new EllipseAnnotation
            {
                Width = 1,   // Diameter of the dot
                Height = 1,
                BackColor = System.Drawing.Color.Black,
                LineColor = System.Drawing.Color.Black,
                AnchorX = x,  // X-axis position
                AnchorY = y,  // Y-axis position
                AnchorAlignment = System.Drawing.ContentAlignment.MiddleCenter
            };
            addition.AxisX = chart_cs.ChartAreas[0].AxisX;
            addition.AxisY = chart_cs.ChartAreas[0].AxisY;
            chart_cs.Annotations.Add(addition);
        }

        private void redrawLines(double[] levels) {
            chart_cs.ChartAreas["ChartArea1"].AxisY.StripLines.Clear();
            foreach (double level in levels) {
                StripLine line = new StripLine
                {
                    Interval = 0, // Ensures it's a single line
                    IntervalOffset = level,  // Y value for the line based of candlesticks and whether it's a peak or a valley
                    StripWidth = 0.01,
                    BackColor = Color.Black // Line color based on type of extreme
                };
                chart_cs.ChartAreas["ChartArea1"].AxisY.StripLines.Add(line);
            }
        }
        /**************************
         *
         *      Event Handlers
         *
         **************************/

        private void button_update_Click(object sender, EventArgs e)
        {
            //calls functions to filter candlesticks to date range and display new filtered list (still from the binding list).
            filterCandlesticks();
            displayCandleSticks();
        }

        private void chart_cs_MouseDown(object sender, MouseEventArgs e)
        {
            int areaTopX = this.chart_cs.Location.X + (int)((this.chart_cs.ChartAreas[0].Position.X / 100) * this.chart_cs.Width);
            int areaTopY = this.chart_cs.Location.Y + (int)((this.chart_cs.ChartAreas[0].Position.Y / 100) * this.chart_cs.Height);
            int areaBottomX = areaTopX + (int)(this.chart_cs.Width * (this.chart_cs.ChartAreas[0].Position.Width / 100));
            int areaBottomY = areaTopY + (int)(this.chart_cs.Height * (this.chart_cs.ChartAreas[0].Position.Height / 100));
            this.chartAreaTop = new Point(areaTopX, areaTopY);  
            this.chartAreaBottom = new Point(areaBottomX, areaBottomY);
            if (inCandlestickBounds(e.Location) && !(this.src is null)) {
                var y = chart_cs.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                Debug.WriteLine($"${y}");
                this.selecting = true;
                this.initial = e.Location;
                this.selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                chart_cs.Annotations.Clear();
                chart_cs.ChartAreas[0].AxisY.StripLines.Clear();
            }
        }


        private void Form_Input_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.selecting)
            {

                markWave();
                this.selectionRectangle.Width = 0;
                this.selectionRectangle.Height = 0;
                this.Refresh(); 
                this.selecting = false;
            }
        }

        private Rectangle transform(Point p) {
            int w = Math.Abs(initial.X - p.X);
            int h = Math.Abs(initial.Y - p.Y);
            int x = Math.Min(p.X, initial.X);
            int y = Math.Min(p.Y, initial.Y);
            return new Rectangle(x, y, w, h);
                           
        }

        private void chart_cs_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.selecting && inCandlestickBounds(e.Location))
            {
                this.selectionRectangle = transform(e.Location);
                this.Refresh();
                
            }
        }

        private void chart_cs_MouseUp(object sender, MouseEventArgs e)
        {
            Form_Input_MouseUp(sender, e);
        }

        private void chart_cs_Paint(object sender, PaintEventArgs e)
        {       
            if (this.selecting) {
                using (Pen pen = new Pen(Color.Black))
                {
                    e.Graphics.DrawRectangle(pen, this.selectionRectangle);
                }
            }
        }

        private void chart_cs_Click(object sender, EventArgs e)
        {

        }
    }
}
