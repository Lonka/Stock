using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stock
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<AbortableBackgroundWorker> mGetStocks = new List<AbortableBackgroundWorker>();
        private bool mGetStockStopFlag = true;
        private List<StockItem> stockItems = new List<StockItem>();
        public MainWindow()
        {
            InitializeComponent();

            stockItems = GetAllSections();
            foreach (StockItem stockItem in stockItems)
            {
                AbortableBackgroundWorker getStock = new AbortableBackgroundWorker();
                mGetStocks.Add(getStock);
                getStock.DoWork += (s, o) =>
                {
                    double openPrice = StockHelper.GetOpenPrice(stockItem.Code);
                    TextBlock txtPrice = null;
                    // 停止的 Flag （全域變數）
                    while (mGetStockStopFlag)
                    {
                        try
                        {
                            if (txtPrice == null)
                            {
                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    // 取得給成交價的 TextBlock 控制項
                                    txtPrice = FindVisualChildByName<TextBlock>(gridStocks, "txtPrice" + stockItem.Code);
                                });
                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                double quotedMarketPrice = StockHelper.GetQuotedMarketPrice(stockItem.Code);

                                #region font color
                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    if (quotedMarketPrice == openPrice)
                                    {
                                        txtPrice.Foreground = Brushes.Black;
                                    }
                                    else if (quotedMarketPrice > openPrice)
                                    {
                                        txtPrice.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xC6, 0x33, 0x00));
                                    }
                                    else if (quotedMarketPrice < openPrice)
                                    {
                                        txtPrice.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xAA, 0x55));
                                    }
                                    txtPrice.Text = quotedMarketPrice.ToString();
                                });
                                #endregion

                                #region Alarm
                                string alarmStr = string.Empty;
                                if (stockItem.AlarmMore != null)
                                {
                                    if (quotedMarketPrice >= stockItem.AlarmMore && stockItem.AlarmMoreMax > 0 && DateTime.Now.Subtract(stockItem.AlarmMoreTime).Seconds > 30)
                                    {
                                        stockItem.AlarmMoreMax--;
                                        stockItem.AlarmMoreTime = DateTime.Now;
                                        alarmStr = DateTime.Now.ToString("HH:mm:ss") + "-" + stockItem.Code + " : " + quotedMarketPrice + " more then " + stockItem.AlarmMore;
                                    }
                                    else if (quotedMarketPrice < stockItem.AlarmMore)
                                    {
                                        stockItem.AlarmMoreMax = 5;
                                    }
                                }
                                if (stockItem.AlarmLess != null)
                                {
                                    if (quotedMarketPrice <= stockItem.AlarmLess && stockItem.AlarmLessMax > 0 && DateTime.Now.Subtract(stockItem.AlarmLessTime).Seconds > 30)
                                    {
                                        stockItem.AlarmLessMax--;
                                        stockItem.AlarmLessTime = DateTime.Now;
                                        alarmStr = DateTime.Now.ToString("HH:mm:ss") + "-" + stockItem.Code + " : " + quotedMarketPrice + " less then " + stockItem.AlarmLess;
                                    }
                                    else if (quotedMarketPrice > stockItem.AlarmLess)
                                    {
                                        stockItem.AlarmLessMax = 5;
                                    }
                                }
                                if (!string.IsNullOrEmpty(alarmStr))
                                {
                                    BackgroundWorker alarmThread = new BackgroundWorker();
                                    alarmThread.DoWork += (ss, oo) =>
                                    {
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            MessageBox.Show(alarmStr, "Alarm", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        });
                                    };
                                    alarmThread.RunWorkerAsync();
                                }
                                #endregion
                            }
                        }
                        catch
                        {

                        }
                        if (DateTime.Now.Hour >= 9 && int.Parse(DateTime.Now.ToString("HHmm")) <= 1330)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(60 * 30 * 1000);
                        }
                    }
                };
                getStock.RunWorkerAsync();
            }
        }

        private List<StockItem> GetAllSections()
        {
            List<StockItem> stockItems = new List<StockItem>();
            List<string> sectionNames = new List<string>();
            string fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setting.ini");
            if (!File.Exists(fileName))
            {
                return stockItems;
            }
            using (StreamReader sr = new StreamReader(fileName, System.Text.Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.StartsWith("[Stock") && line.EndsWith("]"))
                    {
                        sectionNames.Add(line.Replace("[", string.Empty).Replace("]", string.Empty));
                    }
                }
            }
            foreach (string sectionName in sectionNames)
            {
                string code = IniHelper.GetProfileString(fileName, sectionName, "Code", "0050");
                StockItem stockItem = new StockItem(code);
                string alarmMoreThenStr = IniHelper.GetProfileString(fileName, sectionName, "AlarmMoreThen", "");
                double alarmMoreThen = -1;
                if (!string.IsNullOrEmpty(alarmMoreThenStr) && double.TryParse(alarmMoreThenStr, out alarmMoreThen))
                {
                    stockItem.AlarmMore = alarmMoreThen;
                }
                string alarmLessThenStr = IniHelper.GetProfileString(fileName, sectionName, "AlarmLessThen", "");
                double alarmLessThen = -1;
                if (!string.IsNullOrEmpty(alarmLessThenStr) && double.TryParse(alarmLessThenStr, out alarmLessThen))
                {
                    stockItem.AlarmLess = alarmLessThen;
                }

                stockItems.Add(stockItem);
            }
            return stockItems;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rect workAreaRect = SystemParameters.WorkArea;
            this.Left = workAreaRect.Right - ((60 * stockItems.Count) + 10) - 2;
            this.Top = workAreaRect.Bottom - 40 - 2;
            this.Width = (60 * stockItems.Count) + 10;

            for (int i = 0; i < stockItems.Count; i++)
            {
                TextBlock txtCode = new TextBlock();
                txtCode.Text = stockItems[i].Code;
                txtCode.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                txtCode.TextAlignment = TextAlignment.Center;
                txtCode.FontSize = 10;
                txtCode.Width = 50;
                txtCode.Height = 14;
                txtCode.Name = "txtCode" + stockItems[i].Code;
                txtCode.Margin = new Thickness(i * 50, 2, 0, 0);
                txtCode.SetValue(Grid.RowProperty, 0);

                TextBlock txtPrice = new TextBlock();
                txtPrice.Text = "Wait..";
                txtPrice.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                txtPrice.TextAlignment = TextAlignment.Center;
                txtPrice.FontSize = 16;
                txtPrice.Width = 50;
                txtPrice.Height = 24;
                txtPrice.Name = "txtPrice" + stockItems[i].Code;
                txtPrice.Margin = new Thickness(i * 50, 2, 0, 0);
                txtPrice.SetValue(Grid.RowProperty, 1);

                gridStocks.Children.Add(txtCode);
                gridStocks.Children.Add(txtPrice);
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //close thread
            mGetStockStopFlag = false;

            foreach (AbortableBackgroundWorker abortableBackgroundWorker in mGetStocks)
            {
                if (abortableBackgroundWorker.IsBusy == true)
                {
                    abortableBackgroundWorker.Abort();
                    abortableBackgroundWorker.Dispose();
                }
            }
            this.Close();
        }

        /// <summary>
        /// Find Control
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">parent control</param>
        /// <param name="name">child name</param>
        /// <returns></returns>
        public T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);

                    if (result != null)
                        return result;
                }
            }
            return null;
        }

    }
}
