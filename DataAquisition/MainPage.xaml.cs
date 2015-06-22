using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml.Linq; //XElement & Descendants
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using PhoneApp1.Resources;

namespace PhoneApp1
{    
    public partial class MainPage : PhoneApplicationPage
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private Feed feed;
        public ObservableCollection<LineData> LineDataCollection = new ObservableCollection<LineData>();

        public MainPage()
        {
            InitializeComponent();
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(2500);
            timer.Tick += new EventHandler(timer_tick);
        }

        private void timer_tick(object sender, EventArgs e)
        {
            string ui = "select * from csv where url='http://download.finance.yahoo.com/d/quotes.csv?s=" +
                tbSymbol.Text + "&f=sl1d1t1c1ohgv&e=.csv' and columns='symbol,price,date,time,change,open,high,low,volume'";

            WebClient wc = new WebClient();
            wc.OpenReadCompleted += wc_OpenReadCompleted;
            wc.OpenReadAsync(new Uri("https://query.yahooapis.com/v1/public/yql?q=" + HttpUtility.UrlEncode(ui)));

            if (feed != null)
            {
                if (feed.Open != 0 && (feed.Date.Year == DateTime.Now.Year && feed.Date.Month == DateTime.Now.Month && feed.Date.Day == DateTime.Now.Day))
                {
                    (Application.Current as App).D.Add(feed); //add new item to list
                    Comparison<Feed> comp = new Comparison<Feed>(Feed.CompareVolume);
                    (Application.Current as App).D.Sort(comp); //highest Volume first

                    DataList.ItemsSource = null;
                    DataList.ItemsSource = (Application.Current as App).D;

                    LineData ld = new LineData() { Category = ((Application.Current as App).D.Count - 1).ToString(), Price = feed.Price };

                    Decimal low = (Application.Current as App).D.Min(l => l.Low); //find the lowest value in list
                    Decimal high = (Application.Current as App).D.Max(h => h.High); //find highest value in list

                    //check to see if a new low exists
                    if (feed.Low < low)
                        ld.Low = feed.Low;
                    else
                        ld.Low = low;

                    if (feed.High < high)
                        ld.High = feed.High;
                    else
                        ld.High = high;

                    LineDataCollection.Add(ld);
                    LineChart.DataSource = null;
                    LineChart.DataSource = LineDataCollection;
                }
            }
        }

        private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            using (Stream s = e.Result)
            {
                XDocument xmlDoc = XDocument.Load(s);
                List<object> res = (from r in xmlDoc.Descendants("results") where !String.IsNullOrEmpty(r.Value) select r).ToList<object>();

                if (res.Count > 0)
                {
                    XElement elem = (from f in xmlDoc.Descendants("results").Descendants("row") select f).Single();

                    DateTime time = Convert.ToDateTime(elem.Element("time").Value);
                    DateTime date = Convert.ToDateTime(elem.Element("date").Value);

                    feed = new Feed();

                    feed.Symbol = elem.Element("symbol").Value;
                    feed.Price = Convert.ToDecimal(elem.Element("price").Value);
                    feed.Date = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                    feed.Change = Convert.ToDecimal(elem.Element("change").Value);
                    feed.Open = Convert.ToDecimal(elem.Element("open").Value);
                    feed.High = Convert.ToDecimal(elem.Element("high").Value);
                    feed.Low = Convert.ToDecimal(elem.Element("low").Value);
                    feed.Volume = Convert.ToInt64(elem.Element("volume").Value);
                }
            }
        }

        private void cbStart_Checked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(tbSymbol.Text))
                timer.Start();
            else
                cbStart.IsChecked = false;
        }

        private void cbStart_Unchecked(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
                timer.Stop();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Document Acquisition", new List<string>() {".feed"});
            savePicker.SuggestedFileName = DateTime.Now.ToShortDateString().Replace('/', '-') + "-" + tbSymbol.Text;
            savePicker.PickSaveFileAndContinue();
        }
        
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            openPicker.FileTypeFilter.Add(".feed");
            openPicker.PickSingleFileAndContinue();
        }
    }
}