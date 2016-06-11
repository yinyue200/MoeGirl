using MoeGirl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace MoeGirl
{
    public partial class MainPage:Page
    {
        private readonly ObservableCollection<Entry> _listLatest = new ObservableCollection<Entry>();
        private readonly ObservableCollection<Entry> _listSearch = new ObservableCollection<Entry>();
        private ScrollViewer _svSearch;
        private readonly Regex _regexLatestEntry = new Regex("<a href=\"/(?<href>.*?)\" title=\"(?<title>.*?)\"");
        private string _searchContent;

        public MainPage()
        {
            InitializeComponent();
            lbLatest.ItemsSource = _listLatest;
            lbSearch.ItemsSource = _listSearch;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;
            SetBackground();
            GetMainPage();

            tbVersion.Text = "v" + yfxsApp.runtime.Version.GetThisAppVersionString();
        }

        private void SetBackground()
        {
            ImageBrush ib = new ImageBrush();
            ib.Opacity = 0.2;
            BitmapImage bmp = new BitmapImage();
            ib.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Cirno_have_question.png", UriKind.Absolute));
            ib.Stretch = Stretch.Uniform;
            ib.Opacity = 0.1;
            gridBackground.Background = ib;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //lbNewsBox.SelectedIndex = -1;
            lbLatest.SelectedIndex = -1;
            lbSearch.SelectedIndex = -1;
            base.OnNavigatedTo(e);
        }

        //private void lbNewsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (lbNewsBox.SelectedIndex >= 0)
        //    {
        //        Entry en = (Entry)lbNewsBox.SelectedItem;
        //        if (!string.IsNullOrWhiteSpace(en.Href))
        //        {
        //            string destination = "/EntryPage.xaml?href=" + en.Href;
        //            lbLatest.SelectedIndex = -1;
        //            this.Dispatcher.BeginInvoke(() =>
        //            {
        //                try
        //                {
        //                    this.NavigationService.Navigate(new Uri(destination, UriKind.Relative));
        //                }
        //                catch { }
        //            });
        //        }
        //    }
        //}

        private void hbLatest_Click(object sender, RoutedEventArgs e)
        {
            Entry en = (Entry)(sender as HyperlinkButton).DataContext;
            if (!string.IsNullOrWhiteSpace(en.Href))
            {
                string destination = "?href=" + en.Href;
                lbLatest.SelectedIndex = -1;
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        Frame.Navigate(typeof(EntryPage ),destination);
                    }
                    catch { }
                });
            }
        }

        private void hbSearch_Click(object sender, RoutedEventArgs e)
        {
            Entry en = (Entry)(sender as HyperlinkButton).DataContext;
            if (!string.IsNullOrWhiteSpace(en.Href))
            {
                string destination = "?href=" + WebUtility.UrlEncode(en.Href);
                lbLatest.SelectedIndex = -1;
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        Frame.Navigate(typeof(EntryPage ),destination);
                    }
                    catch { }
                });
            }
        }

        private void tboxSearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    string content = tboxSearch.Text.Trim();
                    tbSearch.Visibility = Visibility.Collapsed;
                    tbTotal.Text = string.Empty;

                    if (content.Length > 0)
                    {
                        gridStats.Visibility = Visibility.Visible;
                        Search(content);
                        lbSearch.Focus(FocusState.Programmatic);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("tboxSearch_KeyDown()出错：" + ex.Message);
            }
        }

        private void tboxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tboxSearch.SelectAll();
        }

        private void GetMainPage()
        {
            /*ProgressIndicator pi = new ProgressIndicator();
            pi.Text = "正在获取首页信息";
            pi.IsIndeterminate = true;
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);*/

            string url = "http://zh.moegirl.org/Mainpage";
            Debug.WriteLine(url);
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(new Uri(url));
            hwr.BeginGetResponse(GetMainPageCallback, hwr);
        }

        private void GetMainPageCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);

                using (Stream streamResponse = response.GetResponseStream())
                {
                    string str = new StreamReader(streamResponse).ReadToEnd();

                    Regex regexLatestData = new Regex(">最新条目</div>[\\s\\S]*?</p><p>.*?(?<data>[\\s\\S]*?)</div>");
                    Match matchLatestData = regexLatestData.Match(str);
                    if (matchLatestData.Success)
                    {
                        MatchCollection mc = _regexLatestEntry.Matches(matchLatestData.Groups["data"].Value);

                        for (int i = 0; i < mc.Count; i++)
                        {
                            if (i % 2 == 0)
                                System.Threading.Tasks.Task.Delay(20).Wait();
                            Entry en = new Entry
                            {
                                Href = mc[i].Groups["href"].Value,
                                Title = WebUtility.HtmlDecode(WebUtility.HtmlDecode(mc[i].Groups["title"].Value))
                            };
                            var item = en;
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => _listLatest.Add(item));
                        }
                    }
                }

                //Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SystemTray.SetProgressIndicator(this, null));//TODO
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetSubImagesCompleted(): " + ex.Message);
            }
        }

        private void Search(string content)
        {
            _listSearch.Clear();
            if (_svSearch != null)
                _svSearch.ScrollToVerticalOffset(0);

            _searchContent = content;

            string url = "http://zh.moegirl.org/index.php?title=Special%3A%E6%90%9C%E7%B4%A2&limit=500&offset=0&profile=default&search=" + WebUtility.UrlEncode(content) + "&fulltext=Search";
            Debug.WriteLine(url);

            /*ProgressIndicator pi = new ProgressIndicator
            {
                Text = "正在获取搜索结果",
                IsIndeterminate = true,
                IsVisible = true
            };
            SystemTray.SetProgressIndicator(this, pi);
            rectSearch.Visibility = Visibility.Visible;
            tboxSearch.IsHitTestVisible = false;*/

            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(new Uri(url));
            hwr.BeginGetResponse(SearchCallback, hwr);
        }

        private void SearchCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                using (Stream streamResponse = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(streamResponse))
                    {
                        string result = reader.ReadToEnd();
                        Regex regexSearchResult = new Regex("<div class='searchresults'(?<data>[\\s\\S]*?)</ul>");
                        Match matchSearchResult = regexSearchResult.Match(result);
                        if (matchSearchResult.Success)
                        {
                            string data = matchSearchResult.Groups["data"].Value;

                            try
                            {
                                MatchCollection mc = _regexLatestEntry.Matches(data);

                                int itemcounter = 0;
                                foreach (Match m in mc)
                                {
                                    itemcounter++;
                                    if (itemcounter % 2 == 0)
                                        System.Threading.Tasks.Task.Delay(20).Wait();

                                    if (m.Value.Contains("#."))
                                        continue;

                                    string title = WebUtility.HtmlDecode(m.Groups["title"].Value);
                                    if (string.IsNullOrWhiteSpace(title) || !title.Contains(_searchContent))
                                        continue;

                                    Entry en = new Entry
                                    {
                                        Href = WebUtility.HtmlDecode(title.Replace(' ', '_')),
                                        Title = WebUtility.HtmlDecode(title)
                                    };
                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => _listSearch.Add(en));
                                }
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => panorama.Focus( FocusState.Programmatic));
                            }
                            finally
                            {
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    tbTotal.Text = "共获得" + _listSearch.Count + "条搜索结果";
                                });
                            }
                        }
                        else
                        {
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                tbSearch.Text = "找不到和查询相匹配的结果。";
                                tbSearch.Visibility = Visibility.Visible;
                            });
                        }
                    }
                }
                //Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() => SystemTray.SetProgressIndicator(this, null));//TODO:
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SearchCallback()出错：" + ex.Message);
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
                {
                    /*ProgressIndicator pi = new ProgressIndicator();
                    pi.Text = "搜索时出现错误";
                    pi.IsIndeterminate = false;
                    pi.IsVisible = true;
                    SystemTray.SetProgressIndicator(this, pi);todo*/
                });
            }
            finally
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
                {
                    rectSearch.Visibility = Visibility.Collapsed;
                    tboxSearch.IsHitTestVisible = true;
                });
            }
        }

        private void panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panorama.SelectedIndex == 1)
                tboxSearch.IsHitTestVisible = true;
            else
                tboxSearch.IsHitTestVisible = false;
        }

        private void hbRate_Click(object sender, RoutedEventArgs e)
        {
            //TODO:评价
        }

        private void hbMoreACG_Click(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://search/?query=ACG"));
        }

    }
}
