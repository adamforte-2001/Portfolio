namespace Project1
{
    partial class Form_Input
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Button_LoadStocks = new System.Windows.Forms.Button();
            this.getCSV_openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.DateTimePicker_Start = new System.Windows.Forms.DateTimePicker();
            this.DateTimePicker_End = new System.Windows.Forms.DateTimePicker();
            this.Label_Start = new System.Windows.Forms.Label();
            this.Label_End = new System.Windows.Forms.Label();
            this.chart_cs = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button_update = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chart_cs)).BeginInit();
            this.SuspendLayout();
            // 
            // Button_LoadStocks
            // 
            this.Button_LoadStocks.Location = new System.Drawing.Point(769, 698);
            this.Button_LoadStocks.Margin = new System.Windows.Forms.Padding(2);
            this.Button_LoadStocks.Name = "Button_LoadStocks";
            this.Button_LoadStocks.Size = new System.Drawing.Size(87, 31);
            this.Button_LoadStocks.TabIndex = 0;
            this.Button_LoadStocks.Text = "Choose File";
            this.Button_LoadStocks.UseVisualStyleBackColor = true;
            this.Button_LoadStocks.Click += new System.EventHandler(this.Button_LoadStocks_Click);
            // 
            // getCSV_openFileDialog
            // 
            this.getCSV_openFileDialog.FileName = "openFileDialog1";
            this.getCSV_openFileDialog.Multiselect = true;
            this.getCSV_openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.getCSV_openFileDialog_FileOk);
            // 
            // DateTimePicker_Start
            // 
            this.DateTimePicker_Start.Location = new System.Drawing.Point(578, 703);
            this.DateTimePicker_Start.Margin = new System.Windows.Forms.Padding(2);
            this.DateTimePicker_Start.Name = "DateTimePicker_Start";
            this.DateTimePicker_Start.Size = new System.Drawing.Size(151, 20);
            this.DateTimePicker_Start.TabIndex = 1;
            this.DateTimePicker_Start.Value = new System.DateTime(2024, 10, 1, 0, 0, 0, 0);
            // 
            // DateTimePicker_End
            // 
            this.DateTimePicker_End.Location = new System.Drawing.Point(891, 703);
            this.DateTimePicker_End.Margin = new System.Windows.Forms.Padding(2);
            this.DateTimePicker_End.Name = "DateTimePicker_End";
            this.DateTimePicker_End.Size = new System.Drawing.Size(151, 20);
            this.DateTimePicker_End.TabIndex = 2;
            // 
            // Label_Start
            // 
            this.Label_Start.AutoSize = true;
            this.Label_Start.Location = new System.Drawing.Point(632, 676);
            this.Label_Start.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Label_Start.Name = "Label_Start";
            this.Label_Start.Size = new System.Drawing.Size(29, 13);
            this.Label_Start.TabIndex = 3;
            this.Label_Start.Text = "Start";
            // 
            // Label_End
            // 
            this.Label_End.AutoSize = true;
            this.Label_End.Location = new System.Drawing.Point(961, 676);
            this.Label_End.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Label_End.Name = "Label_End";
            this.Label_End.Size = new System.Drawing.Size(26, 13);
            this.Label_End.TabIndex = 4;
            this.Label_End.Text = "End";
            // 
            // chart_cs
            // 
            chartArea7.AxisX.MajorGrid.Enabled = false;
            chartArea7.AxisY.MajorGrid.Enabled = false;
            chartArea7.Name = "ChartArea1";
            chartArea8.AxisX.Interval = 1D;
            chartArea8.AxisX.MajorGrid.Enabled = false;
            chartArea8.AxisY.MajorGrid.Enabled = false;
            chartArea8.Name = "ChartArea2";
            this.chart_cs.ChartAreas.Add(chartArea7);
            this.chart_cs.ChartAreas.Add(chartArea8);
            legend4.Enabled = false;
            legend4.Name = "Legend1";
            this.chart_cs.Legends.Add(legend4);
            this.chart_cs.Location = new System.Drawing.Point(11, 11);
            this.chart_cs.Margin = new System.Windows.Forms.Padding(2);
            this.chart_cs.Name = "chart_cs";
            series7.ChartArea = "ChartArea1";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series7.Legend = "Legend1";
            series7.Name = "Series1";
            series7.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series7.YValuesPerPoint = 4;
            series7.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series8.BorderWidth = 3;
            series8.ChartArea = "ChartArea2";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series8.Legend = "Legend1";
            series8.Name = "Series2";
            series8.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series8.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            this.chart_cs.Series.Add(series7);
            this.chart_cs.Series.Add(series8);
            this.chart_cs.Size = new System.Drawing.Size(1653, 643);
            this.chart_cs.TabIndex = 6;
            this.chart_cs.Text = "chart1";
            this.chart_cs.Click += new System.EventHandler(this.chart_cs_Click);
            this.chart_cs.Paint += new System.Windows.Forms.PaintEventHandler(this.chart_cs_Paint);
            this.chart_cs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart_cs_MouseDown);
            this.chart_cs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart_cs_MouseMove);
            this.chart_cs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.chart_cs_MouseUp);
            // 
            // button_update
            // 
            this.button_update.Location = new System.Drawing.Point(1101, 701);
            this.button_update.Margin = new System.Windows.Forms.Padding(2);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(122, 25);
            this.button_update.TabIndex = 7;
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            this.button_update.Click += new System.EventHandler(this.button_update_Click);
            // 
            // Form_Input
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1675, 750);
            this.Controls.Add(this.button_update);
            this.Controls.Add(this.chart_cs);
            this.Controls.Add(this.Label_End);
            this.Controls.Add(this.Label_Start);
            this.Controls.Add(this.DateTimePicker_End);
            this.Controls.Add(this.DateTimePicker_Start);
            this.Controls.Add(this.Button_LoadStocks);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form_Input";
            this.Text = "Stock Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_Input_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.chart_cs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Button_LoadStocks;
        private System.Windows.Forms.OpenFileDialog getCSV_openFileDialog;
        private System.Windows.Forms.DateTimePicker DateTimePicker_Start;
        private System.Windows.Forms.DateTimePicker DateTimePicker_End;
        private System.Windows.Forms.Label Label_Start;
        private System.Windows.Forms.Label Label_End;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_cs;
        private System.Windows.Forms.Button button_update;
    }
}

