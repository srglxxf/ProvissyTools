using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProvissyTools
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            LineChart1.Title = "正在生成统计图，请稍等。。。";
            Thread t = new Thread(initializeChart);
            t.Start();
        }



        //private static Style GetNewDataPointStyle(int R,int G,int B)
        //{
        //    Random random = new Random();
        //    Color background = Color.FromRgb((byte)R,
        //                                     (byte)G,
        //                                     (byte)B);
        //    Style style = new Style(typeof(DataPoint));
        //    Setter st0 = new Setter(LineDataPoint.BackgroundProperty, new SolidColorBrush(background));
        //    Setter st1 = new Setter(DataPoint.BackgroundProperty,
        //                                new SolidColorBrush(background));
        //    Setter st2 = new Setter(DataPoint.BorderBrushProperty,
        //                                new SolidColorBrush(Colors.White));
        //    Setter st3 = new Setter(DataPoint.BorderThicknessProperty, new Thickness(0.1));

        //    Setter st4 = new Setter(DataPoint.TemplateProperty, null);
        //    Setter st5 = new Setter(LineSeries.BackgroundProperty, new SolidColorBrush(background));
        //    style.Setters.Add(st0);
        //    style.Setters.Add(st1);
        //    style.Setters.Add(st2);
        //    style.Setters.Add(st3);
        //    style.Setters.Add(st4);
        //    style.Setters.Add(st5);
        //    return style;
        //}

        private void loadMatChart()
        {
            Action a = new Action(() => {
                List<string[]> loadedList = ReadCSV(UniversalConstants.CurrentDirectory + "MaterialsLog.csv");
            LineSeries fuelLine = LineChart1.Series[0] as LineSeries;
            fuelLine.ItemsSource = loadFuel(loadedList);
            LineSeries ammoLine = LineChart1.Series[1] as LineSeries;
            ammoLine.ItemsSource = loadAmmo(loadedList);
            LineSeries steelLine = LineChart1.Series[2] as LineSeries;
            steelLine.ItemsSource = loadSteel(loadedList);
            LineSeries bauxiteLine = LineChart1.Series[3] as LineSeries;
            bauxiteLine.ItemsSource = loadBauxite(loadedList);
            //Style dataPointStyle1 = GetNewDataPointStyle(34,139,34);
            //Style dataPointStyle2 = GetNewDataPointStyle(138,54,15);
            //Style dataPointStyle3 = GetNewDataPointStyle(128,138,135);
            //Style dataPointStyle4 = GetNewDataPointStyle(199,97,20);
            //fuelLine.DataPointStyle = dataPointStyle1;
            //ammoLine.DataPointStyle = dataPointStyle2;
            //steelLine.DataPointStyle = dataPointStyle3;
            //bauxiteLine.DataPointStyle = dataPointStyle4;
            LineChart1.Title = "资源统计图";
            });
            this.Dispatcher.Invoke(a, DispatcherPriority.ApplicationIdle);
        }

        private List<MatData> loadBauxite(List<string[]> loadMat)
        {
            List<MatData> matdata = new List<MatData>();
            foreach (string[] ss in loadMat)
            {
                matdata.Add(new MatData(ss[0], Int32.Parse(ss[4])));
            }
            return matdata;
        }

        private List<MatData> loadSteel(List<string[]> loadMat)
        {
            List<MatData> matdata = new List<MatData>();
            foreach (string[] ss in loadMat)
            {
                matdata.Add(new MatData(ss[0], Int32.Parse(ss[3])));
            }
            return matdata;
        }

        private List<MatData> loadAmmo(List<string[]> loadMat)
        {
            List<MatData> matdata = new List<MatData>();
            foreach (string[] ss in loadMat)
            {
                matdata.Add(new MatData(ss[0], Int32.Parse(ss[2])));
            }
            return matdata;
        }

        private List<MatData> loadFuel(List<string[]> loadMat)
        {
            List<MatData> matdata = new List<MatData>();
            foreach (string[] ss in loadMat)
            {
                matdata.Add(new MatData(ss[0], Int32.Parse(ss[1])));
            }
            return matdata;
        }



        /// <summary>
        /// CSV File Reader.
        /// Read a csv file to List<string[]> .
        /// </summary>
        /// <param name="filePathName"></param>
        /// <returns></returns>
        public static List<String[]> ReadCSV(string filePathName )
        {
            List<String[]> ls = new List<String[]>();
            StreamReader fileReader = new StreamReader(filePathName);
            string strLine = "";
            fileReader.ReadLine();   //Skip first row.
            while (strLine != null)
            {
                strLine = fileReader.ReadLine();
                if (strLine != null && strLine.Length > 0)
                {
                    ls.Add(strLine.Split(','));
                    //Debug.WriteLine(strLine);
                }
            }
            fileReader.Close();
            return ls;
        }

        private void initializeChart()
        {
            try
            {
                loadMatChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载统计图错误！ " + ex.ToString());
                //MainView m = new MainView();
                //m.ErrorHandler("加载统计图错误！ " + ex.ToString());
            }
        }
    }


    public class MatData
    {
        public string DateOF { get; set; }
        public int countOfMat { get; set; }

        public MatData(string dateof, int countofmat)
        {
            DateOF = dateof;
            countOfMat = countofmat;
        }
    }

    //public class LineSeriesEx : LineSeries
    //{
    //    protected override DataPoint CreateDataPoint()
    //    {
    //        return new EmptyDataPoint();
    //    }
    //}

    //public class EmptyDataPoint : DataPoint
    //{
    //    // As the method name says, this DataPoint is empty.
    //}
}
