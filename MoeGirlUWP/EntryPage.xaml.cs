
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using yfxsApp.Common;
using yfxsApp.runtime;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MoeGirl
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EntryPage : BasicPage
    {
        //private ApplicationBarMenuItem menuRefresh;

        private string _href = string.Empty;
        private string _title = string.Empty;
        private string _wbStr = string.Empty;
        private string _urlCoverImg = string.Empty;
        private string _currentAnchor;
        //private Regex regexImage = new Regex("<img (alt=\"(?<alt>.*?)\")?.*?static.mengniang.org/(thumb/)?(?<urlpart>(\\w+/)+)(\\d{3,}x){0,2}(?<filename>.*?(\\.(jpg|jpeg|png|gif|bmp))+)", RegexOptions.IgnoreCase);
        //private readonly Regex _regexImage = new Regex("/(?<url1>(.*moegirl).org/)(thumb/)?(?<url2>.*?/.*?/)(\\d+x\\d+x)?(?<url3>.*?\\.(jpg|jpeg|png|gif|bmp))", RegexOptions.IgnoreCase);
        //private Regex regexImagePrefix = new Regex("/(?<url1>(static.mengniang|img.moegirl).org/)(thumb/)?(?<url2>.*?/.*?/)");
        private readonly Regex _regexImage = new Regex("/(?<url1>((.*mengniang|.*moegirl).org|img.acg.moe)/)(thumb/|File:)?(?<url2>.*?/.*?/)(\\d+x\\d+x)?(?<url3>.*?\\.(jpg|jpeg|png|gif|bmp))", RegexOptions.IgnoreCase);
        //private Regex regexImageFileName = new Regex("<a href=\"/File:(?<filename>.*?)\"");
        private readonly Regex _regexHref = new Regex("<a href=\"(?<href>.*?)\".*?>");
        private readonly Regex _regexSpan = new Regex("<span .*?>");

        public EntryPage()
        {
            InitializeComponent();

            /*ApplicationBarMenuItem menuHome = new ApplicationBarMenuItem("回到首页");
            menuHome.Click += menuHome_Click;
            ApplicationBarMenuItem menuWebView = new ApplicationBarMenuItem("在浏览器中阅读条目");
            menuWebView.Click += menuWebView_Click;
            //menuRefresh = new ApplicationBarMenuItem("刷新详细介绍");
            //menuRefresh.Click += menuRefresh_Click;
            ApplicationBarMenuItem menuShare = new ApplicationBarMenuItem("分享条目");
            menuShare.Click += menuShare_Click;
            ApplicationBarMenuItem menuSendReport = new ApplicationBarMenuItem("发送问题报告");
            menuSendReport.Click += menuSendReport_Click;

            ApplicationBar.MenuItems.Add(menuHome);
            ApplicationBar.MenuItems.Add(menuWebView);
            //ApplicationBar.MenuItems.Add(menuRefresh);
            ApplicationBar.MenuItems.Add(menuShare);
            ApplicationBar.MenuItems.Add(menuSendReport);

            wbDetails.NavigateToString("<html><head></head><body bgcolor=\"f3f3f3\"></body></html>");*/
        }
        protected override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var parameters = e.ParameterDictionary;
            if (parameters.ContainsKey("href"))
            {
                _href = parameters["href"];
                tbTitle.Text = _href.Replace('_', ' ');
                _title = tbTitle.Text;
                e.ParameterDictionary.Remove("href");
                if (!string.IsNullOrEmpty(_href))
                    GetWebpage(_href);
            }
            //SystemTray.IsVisible = !Orientation.ToString().StartsWith("Landscape");
        }
        /*protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = NavigationContext.QueryString;
            if (parameters.ContainsKey("href"))
            {
                _href = parameters["href"];
                tbTitle.Text = _href.Replace('_', ' ');
                _title = tbTitle.Text;
                NavigationContext.QueryString.Remove("href");
                if (!string.IsNullOrEmpty(_href))
                    GetWebpage(_href);
            }
            SystemTray.IsVisible = !Orientation.ToString().StartsWith("Landscape");
            base.OnNavigatedTo(e);
        }*/

        /*protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            SystemTray.IsVisible = !e.Orientation.ToString().StartsWith("Landscape");
            ComputeScrolling();
            base.OnOrientationChanged(e);
        }*/


        private void wbDetails_Navigating(object sender, WebViewNavigationStartingEventArgs e)
        {
            if(e.Uri==null)
            {
                return;
            }
            string url = e.Uri.OriginalString;
            if (url.StartsWith("about:"))
                url = url.Replace("about:", string.Empty);

            if (url.StartsWith("blank#"))
            {
                _currentAnchor = url.Substring(6);
                if (!string.IsNullOrWhiteSpace(_currentAnchor))
                    wbDetails.LoadCompleted += wbDetails_LoadCompleted;
            }
            else
            {
                e.Cancel = true;
                if (url.Contains("action=edit&"))
                {
                    /*ProgressIndicator pi = new ProgressIndicator { Text = "页面不存在", IsIndeterminate = false, IsVisible = true };
                    SystemTray.SetProgressIndicator(this, pi);*/
                }
                else if (url.StartsWith("/") && !url.Contains("index.php") && !url.Contains("/File:") && !url.Contains("/Category:") && !url.Contains("/Help:") && !url.Contains("/Special:"))
                {
                    Regex regexHref = new Regex("/(?<href>.*)");
                    Match matchHref = regexHref.Match(url);
                    if (matchHref.Success)
                    {
                        string href = matchHref.Groups["href"].Value;
                        string destination = "?href=" + href;
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                Frame.Navigate(typeof(EntryPage), destination);
                            }
                            catch { }
                        });
                    }
                }
                else
                {
                    if (url.StartsWith("/"))
                        url = "http://zh.moegirl.org" + url;
                    Windows.System.Launcher.LaunchUriAsync(new Uri(url, UriKind.Absolute));
                }
            }
        }

        private void wbDetails_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            /*ProgressIndicator pi = new ProgressIndicator { Text = "页面加载失败", IsIndeterminate = false, IsVisible = true };
            SystemTray.SetProgressIndicator(this, pi);*/
        }

        private void wbDetails_LoadCompleted(object sender, NavigationEventArgs e)
        {
            wbDetails.LoadCompleted -= wbDetails_LoadCompleted;
            try
            {
                wbDetails.InvokeScript("relocate", new string[] { _currentAnchor });
                _currentAnchor = string.Empty;
            }
            catch { }
        }

        /*private void wbDetails_Flick(object sender, FlickGestureEventArgs e)
        {
            //Debug.WriteLine(e.Angle + " " + e.Direction + " " + e.Handled + " " + e.HorizontalVelocity + " " + e.VerticalVelocity);
            if (e.Direction == Orientation.Horizontal)
            {
                pivot.SelectedIndex = e.HorizontalVelocity < 0 ? 2 : 0;
            }
        }*/

        private void GetWebpage(string href)
        {
            /*ProgressIndicator pi = new ProgressIndicator { Text = "页面加载中", IsIndeterminate = true, IsVisible = true };
            SystemTray.SetProgressIndicator(this, pi);*/
            string url = "http://zh.moegirl.org/" + WebUtility.UrlEncode(href);
            Debug.WriteLine(url);
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(new Uri(url));
            hwr.BeginGetResponse(GetWebpageCallback, hwr);
        }

        private void GetWebpageCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                using (Stream streamResponse = response.GetResponseStream())
                {
                    string result = new StreamReader(streamResponse).ReadToEnd();

                    Regex regexData = new Regex("(?<data><h1 id=\"firstHeading\"[\\s\\S]*?)<div class=\"printfooter\">");
                    Match matchData = regexData.Match(result);
                    if (matchData.Success)
                    {
                        string data = matchData.Groups["data"].Value.Trim();
                        if (data.Contains("firstHeading"))
                        {
                            //data = HtmlHelper.ColorHtmlBody(data, HtmlHelper.IsDarkTheme());

                            // 移除width设置
                            Regex regexWidth = new Regex("width(=|:)(\")?\\d*(px)?(\")?");
                            MatchCollection mcWidth = regexWidth.Matches(data);
                            foreach (Match m in mcWidth)
                                data = data.Replace(m.Value, string.Empty);

                            data = "<html lang=\"zh\" dir=\"ltr\" class=\"client-nojs\">\n<head><meta charset=\"UTF-8\"/><meta name='viewport' content=\"width=device-width, user-scalable=no\"/></head>\n<body bgcolor=\"f3f3f3\" style=\"word-wrap:break-word;\"><script type=\"text/javascript\">function relocate(elID) { var el = document.getElementById(elID); el.scrollIntoView(); }function showImgDelay(imgObj,imgSrc,maxErrorNum){window.external.notify('showImgDelay: '+imgSrc);if(maxErrorNum>0){imgObj.onerror=function(){showImgDelay(imgObj,imgSrc,maxErrorNum-1);};setTimeout(function(){imgObj.src=imgSrc+(maxErrorNum-1);window.external.notify('showImgDelay reload: '+imgObj.src);},500);}else{imgObj.onerror=null;imgObj.src=\"http://1-ps.googleusercontent.com/x/zh.moegirl.org/static.mengniang.org/x2013logo.png.pagespeed.ic.bf0dq7k7pd.webp\";}}</script>" + data;

                            Regex regexTitle = new Regex("<h1 id=\"firstHeading\" class=\"firstHeading\".*?<span.*?>(?<title>.*?)</span></h1>");
                            Match matchTitle = regexTitle.Match(data);
                            if (matchTitle.Success)
                            {
                                string title = matchTitle.Groups["title"].Value;
                                if (!string.IsNullOrWhiteSpace(title))
                                {
                                    _title = WebUtility.HtmlDecode(WebUtility.HtmlDecode(title));
                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        tbTitle.Text = _title;
                                    });
                                }
                                data = data.Replace(matchTitle.Value, string.Empty);
                            }

                            Regex regexSiteSub = new Regex("<div id=\"siteSub\">.*?</div>");
                            Match matchSiteSub = regexSiteSub.Match(data);
                            if (matchSiteSub.Success)
                                data = data.Replace(matchSiteSub.Value, string.Empty);

                            Regex regexBig = new Regex("<big>.*?</big>");
                            Match matchBig = regexBig.Match(data);
                            if (matchBig.Success)
                                data = data.Replace(matchBig.Value, string.Empty);

                            Regex regexJumpToNav = new Regex("<div id=\"jump-to-nav\"[\\s\\S]*?</div>");
                            Match matchJumpToNav = regexJumpToNav.Match(data);
                            if (matchJumpToNav.Success)
                                data = data.Replace(matchJumpToNav.Value, string.Empty);

                            Regex regexCommonBox = new Regex("<(table|div) class=\"common-box( |\")[\\s\\S]*?</\\1>");
                            MatchCollection mcCommonBox = regexCommonBox.Matches(data);
                            foreach (Match m in mcCommonBox)
                                data = data.Replace(m.Value, string.Empty);

                            MatchCollection mcmwContentText = new Regex("<div class=\"infoBox\"[\\s\\S]*?度过愉快的时光[\\s\\S]*?</div></div>").Matches(data);
                            foreach (Match m in mcmwContentText)
                                data = data.Replace(m.Value, string.Empty);

                            Regex regexNoPrint = new Regex("<div class=\"noprint\"[\\s\\S]*?(<div [\\s\\S]*?</div>[\\s\\S]*?)+</div>");
                            MatchCollection mcNoPrint = regexNoPrint.Matches(data);
                            foreach (Match m in mcNoPrint)
                                data = data.Replace(m.Value, string.Empty);

                            Regex regexEmbed = new Regex("<embed [\\s\\S]*?</embed>");
                            MatchCollection mcEmbed = regexEmbed.Matches(data);
                            foreach (Match m in mcEmbed)
                                data = data.Replace(m.Value, string.Empty);

                            //Regex regexMorePics = new Regex("<td colspan=\"\\d\" align=\"center\" bgcolor=\"#CCCCFF\".*?的更多图片</b></a>[\\s\\S]*?</td>");
                            //Match matchMorePics = regexMorePics.Match(data);
                            //if (matchMorePics.Success)
                            //    data = data.Replace(matchMorePics.Value, string.Empty);

                            //Regex regexChaLunBian = new Regex("<div class=\"noprint plainlinks hlist navbar nomobile\".*?</div>");
                            //Match matchChaLunBian = regexChaLunBian.Match(data);
                            //if (matchChaLunBian.Success)
                            //    data = data.Replace(matchChaLunBian.Value, string.Empty);

                            Regex regexEdit = new Regex("\\[.*?<a href=\"/index.php?.*?编辑</a>.*?\\]");
                            MatchCollection mcEdit = regexEdit.Matches(data);
                            foreach (Match m in mcEdit)
                                data = data.Replace(m.Value, string.Empty);

                            // 分离基本资料
                            {
                                Regex regexBasicData = new Regex("<table border=\"(?<basicdata>[\\s\\S]*?)</table>");
                                Match matchBasicData = regexBasicData.Match(data);
                                if (matchBasicData.Success)
                                {
                                    string basicData = matchBasicData.Groups["basicdata"].Value;
                                    data = data.Replace(matchBasicData.Value, string.Empty);

                                    Match matchImageCover = _regexImage.Match(matchBasicData.Value);
                                    if (matchImageCover.Success)
                                    {
                                        string imgUrl = "http://" + matchImageCover.Groups["url1"].Value + matchImageCover.Groups["url2"].Value + matchImageCover.Groups["url3"].Value;
                                        imgUrl = imgUrl.Replace(' ', '_').Replace(",P", "%").Replace("thumb/", string.Empty);
                                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            _urlCoverImg = imgUrl;
                                            imgCover.Source = new BitmapImage(new Uri(imgUrl, UriKind.Absolute));
                                            //miSavePic.IsEnabled = true;//todo
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            tbBasic.Text = "本条目暂未提供封面图片O.O";
                                        });
                                    }

                                    Regex regexTh = new Regex("<th.*?>(?<th>[\\s\\S]*?)</th>");
                                    Regex regexTd = new Regex("<td.*?>(?<td>[\\s\\S]*?)</td>");
                                    Regex regexSpan = new Regex("<span .*?>");
                                    MatchCollection mcRow = new Regex("<tr>[\\s\\S]*?</tr>").Matches(basicData);

                                    try
                                    {
                                        for (int i = 0; i < mcRow.Count; i++)
                                        {
                                            Match m = mcRow[i];

                                            Match matchTh = regexTh.Match(m.Value);
                                            Match matchTd = regexTd.Match(m.Value);
                                            if (matchTh.Success && matchTd.Success)
                                            {
                                                string th = matchTh.Groups["th"].Value;
                                                string td = matchTd.Groups["td"].Value;
                                                if (!string.IsNullOrWhiteSpace(th) && !string.IsNullOrWhiteSpace(td))
                                                {
                                                    th = Utils.RemoveTagsAndDecode(th);
                                                    // 移除所有超链接
                                                    MatchCollection mcA = new Regex("<a .*?>").Matches(td);
                                                    foreach (Match ma in mcA)
                                                    {
                                                        td = td.Replace(ma.Value, string.Empty);
                                                    }
                                                    td = td.Replace("</a>", string.Empty).Trim();
                                                    MatchCollection mcSpan = regexSpan.Matches(td);
                                                    foreach (Match ms in mcSpan)
                                                    {
                                                        td = td.Replace(ms.Value, string.Empty);
                                                    }

                                                    td = Utils.RemoveTagsAndDecode(td);

                                                    System.Threading.Tasks.Task.Delay(20).Wait();

                                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                                    {
                                                        RowDefinition rd = new RowDefinition { Height = GridLength.Auto };
                                                        gridBasic.RowDefinitions.Add(rd);

                                                        TextBlock tbTh = new TextBlock
                                                        {
                                                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55)),
                                                            Margin = new Thickness(0, 0, 5, 5),
                                                            HorizontalAlignment = HorizontalAlignment.Right,
                                                            Text = th
                                                        };
                                                        Grid.SetRow(tbTh, gridBasic.RowDefinitions.Count - 1);
                                                        Grid.SetColumn(tbTh, 0);

                                                        TextBlock tbTd = new TextBlock
                                                        {
                                                            Foreground = new SolidColorBrush(Colors.Black),
                                                            HorizontalAlignment = HorizontalAlignment.Center,
                                                            TextWrapping = TextWrapping.Wrap,
                                                            TextAlignment = TextAlignment.Center,
                                                            Text = td
                                                        };
                                                        Grid.SetRow(tbTd, gridBasic.RowDefinitions.Count - 1);
                                                        Grid.SetColumn(tbTd, 1);

                                                        gridBasic.Children.Add(tbTh);
                                                        gridBasic.Children.Add(tbTd);
                                                    });
                                                }
                                            }
                                        }
                                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            RowDefinition rdSpan = new RowDefinition();
                                            rdSpan.Height = new GridLength(32);
                                            gridBasic.RowDefinitions.Add(rdSpan);
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("分离基本资料出错：" + ex.Message);
                                    }
                                }
                                else
                                {
                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        tbBasic.Text = "本条目暂无基本资料O.O";
                                        if (pivot.SelectedIndex == 0)
                                            pivot.SelectedIndex = 1;
                                    });
                                }
                            }

                            Regex regexMagnify = new Regex("<div class=\"magnify\">.*?</a></div>");
                            MatchCollection mcMagnify = regexMagnify.Matches(data);
                            foreach (Match m in mcMagnify)
                                data = data.Replace(m.Value, string.Empty);

                            // 分离相关条目
                            {
                                Regex regexRelated = new Regex("<table class=\"navbox\".*?<table .*?查</span>(?<relateddata>[\\s\\S]*)</td></tr></table>");
                                Match matchRelated = regexRelated.Match(data);
                                if (matchRelated.Success)
                                {
                                    string relatedData = matchRelated.Groups["relateddata"].Value;
                                    data = data.Replace(matchRelated.Value, string.Empty);

                                    Regex regexGroupName1 = new Regex("\">(?<group1>.*?)</td>");
                                    Regex regexGroupName2 = new Regex("<div style=\"padding:0em 0.75em.*?>(?<group2>.*?)</div>.*?</td></tr>");
                                    Regex regexGroupContent = new Regex("(?<content><(a|strong) .*?>.*?</(a|strong)>)");
                                    Regex regexStrong = new Regex("<strong .*?>(?<data>.*?)</strong>");
                                    MatchCollection mcRow = new Regex("\">.*?(</tr><tr style=\"height:2px;?\"><td></td></tr><tr><td class=\"navbox-group\" style=\";[^p]|(title=\"Template:)|(</div></td></tr></table><div></div></td></tr></table>))").Matches(relatedData);

                                    try
                                    {
                                        foreach (Match m in mcRow)
                                        {
                                            System.Threading.Tasks.Task.Delay(20).Wait();

                                            bool hasIndent = false;
                                            string rowData = m.Value;

                                            Match matchGroupName1 = regexGroupName1.Match(rowData);
                                            string groupName1 = string.Empty;
                                            if (matchGroupName1.Success)
                                            {
                                                groupName1 = matchGroupName1.Groups["group1"].Value;

                                                if (groupName1.Contains(">论<") && groupName1.Contains(">编<"))
                                                    continue;

                                                groupName1 = groupName1.Replace("<br/>", " ").Replace("<br />", " ");

                                                Match matchHref1 = _regexHref.Match(groupName1);
                                                if (matchHref1.Success)
                                                    groupName1 = groupName1.Replace(matchHref1.Value, string.Empty).Replace("</a>", string.Empty);

                                                rowData = rowData.Replace(matchGroupName1.Value, string.Empty);
                                                hasIndent = true;

                                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                                                {
                                                    RowDefinition rd = new RowDefinition { Height = GridLength.Auto };
                                                    gridRelated.RowDefinitions.Add(rd);

                                                    TextBlock tbGroupName1 = new TextBlock
                                                    {
                                                        Margin = new Thickness(5, 5, 0, 5),
                                                        HorizontalAlignment = HorizontalAlignment.Left,
                                                        FontSize = 28,
                                                        FontWeight = FontWeights.SemiBold,
                                                        TextWrapping = TextWrapping.Wrap
                                                    };
                                                    Match matchStrong = regexStrong.Match(groupName1);
                                                    if (matchStrong.Success)
                                                    {
                                                        groupName1 = matchStrong.Groups["data"].Value;
                                                        tbGroupName1.Foreground = (Brush)Application.Current.Resources["ComboBoxItemSelectedBackgroundThemeBrush"];
                                                    }
                                                    else
                                                        tbGroupName1.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55));
                                                    groupName1 = Utils.RemoveTagsAndDecode(groupName1);
                                                    tbGroupName1.Text = groupName1;
                                                    Grid.SetRow(tbGroupName1, gridRelated.RowDefinitions.Count - 1);
                                                    gridRelated.Children.Add(tbGroupName1);
                                                });
                                            }

                                            List<Tuple<string, string>> listRow = new List<Tuple<string, string>>();
                                            MatchCollection matchGroupName2 = regexGroupName2.Matches(rowData);
                                            if (matchGroupName2.Count > 0)
                                            {
                                                hasIndent = true;
                                                listRow.AddRange(from Match match in matchGroupName2 select new Tuple<string, string>(match.Groups["group2"].Value, match.Value));
                                            }
                                            else if (!string.IsNullOrWhiteSpace(matchGroupName1.Value))
                                            {
                                                hasIndent = false;
                                                Tuple<string, string> tuple = new Tuple<string, string>(groupName1, rowData);
                                                listRow.Add(tuple);
                                            }

                                            foreach (Tuple<string, string> tuple in listRow)
                                            {
                                                string groupName2 = tuple.Item1;
                                                string rowStr = tuple.Item2;

                                                Match matchHref2 = _regexHref.Match(groupName2);
                                                if (matchHref2.Success)
                                                    groupName2 = groupName2.Replace(matchHref2.Value, string.Empty).Replace("</a>", string.Empty);

                                                matchHref2 = _regexHref.Match(groupName2);
                                                if (matchHref2.Success)
                                                    groupName2 = groupName2.Replace(matchHref2.Value, string.Empty).Replace("</a>", string.Empty);

                                                groupName2 = Utils.RemoveTagsAndDecode(groupName2);

                                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                                {
                                                    RowDefinition rd = new RowDefinition();

                                                    if (hasIndent)
                                                    {
                                                        rd.Height = GridLength.Auto;
                                                        gridRelated.RowDefinitions.Add(rd);

                                                        TextBlock tbGroupName2 = new TextBlock
                                                        {
                                                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55)),
                                                            Margin = new Thickness(0, 0, 0, 5),
                                                            HorizontalAlignment = HorizontalAlignment.Center,
                                                            FontSize = 20,
                                                            Text = groupName2
                                                        };
                                                        Grid.SetRow(tbGroupName2, gridRelated.RowDefinitions.Count - 1);
                                                        gridRelated.Children.Add(tbGroupName2);

                                                        rd = new RowDefinition();
                                                    }

                                                    rd.Height = GridLength.Auto;
                                                    gridRelated.RowDefinitions.Add(rd);

                                                    BalancedWrapPanel wp = new BalancedWrapPanel { Margin = new Thickness(0, 0, 0, 12), HorizontalAlignment = HorizontalAlignment.Center };
                                                    Grid.SetRow(wp, gridRelated.RowDefinitions.Count - 1);

                                                    MatchCollection mcGroupContent = regexGroupContent.Matches(rowStr);
                                                    foreach (Match mContent in mcGroupContent)
                                                    {
                                                        string groupContent = string.Empty;
                                                        HyperlinkButton hb = new HyperlinkButton();

                                                        if (mContent.Value.StartsWith("<a ") && !mContent.Value.Contains("/File:"))
                                                        {
                                                            Match matchHref3 = _regexHref.Match(mContent.Value);
                                                            if (matchHref3.Success)
                                                            {
                                                                string href = matchHref3.Groups["href"].Value;
                                                                if (href.StartsWith("/"))
                                                                {
                                                                    groupContent = mContent.Value;
                                                                    groupContent = groupContent.Replace(matchHref3.Value, string.Empty).Replace("</a>", string.Empty);

                                                                    if (href.StartsWith("/index.php") || href.StartsWith("/Template") || href.StartsWith("/Category"))
                                                                        hb.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 186, 0, 0));
                                                                    else
                                                                    {
                                                                        hb.DataContext = href;
                                                                        hb.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 6, 69, 173));
                                                                    }
                                                                    hb.Click += hb_Click;
                                                                }
                                                            }
                                                        }
                                                        else if (mContent.Value.StartsWith("<strong "))
                                                        {
                                                            Match matchStrong = regexStrong.Match(mContent.Value);
                                                            if (matchStrong.Success)
                                                            {
                                                                groupContent = matchStrong.Groups["data"].Value;
                                                                hb.Foreground = (Brush)Application.Current.Resources["ComboBoxItemSelectedBackgroundThemeBrush"];
                                                            }
                                                        }

                                                        // 去除span
                                                        string spanValue = _regexSpan.Match(groupContent).Value;
                                                        if (!string.IsNullOrEmpty(spanValue))
                                                            groupContent = groupContent.Replace(spanValue, string.Empty).Replace("</span>", string.Empty);

                                                        if (!string.IsNullOrWhiteSpace(groupContent))
                                                        {
                                                            hb.FontSize = 20;
                                                            hb.Content = groupContent;
                                                            wp.Children.Add(hb);
                                                        }
                                                    }
                                                    if (wp.Children.Count > 0)
                                                        gridRelated.Children.Add(wp);
                                                });
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("分离相关条目出错：" + ex.Message);
                                    }

                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        tbRelated.Visibility = Visibility.Collapsed;
                                    });
                                }
                                else
                                {
                                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        tbRelated.Text = "本条目暂无相关条目O.O";
                                    });
                                }
                            }

                            // 处理带超链接的图片
                            Regex regexImageData = new Regex("<a .*?class=\"image\"[\\s\\S]*?<img.*?(pagespeed_lazy_)?src=\"(?<thumb>.*?)\".*?(onload=\")?.*?</a>");
                            MatchCollection mcImage = regexImageData.Matches(data);
                            foreach (Match m in mcImage)
                            {
                                string imgUrl = Regex.Match(m.Value, "src=\"(?<src>.*?)\"").Groups["src"].Value;
                                string imgRealUrl = imgUrl;
                                Match matchRealImage = _regexImage.Match(m.Value);
                                if (matchRealImage.Success)
                                {
                                    imgRealUrl = "http://" + matchRealImage.Groups["url1"].Value + matchRealImage.Groups["url2"].Value + matchRealImage.Groups["url3"].Value;
                                    imgRealUrl = imgRealUrl.Replace(' ', '_').Replace(",P", "%").Replace("thumb/", string.Empty);
                                }
                                data = data.Replace(m.Value, string.Format("<a href=\"{0}\"><p><img alt=\"\" src=\"{1}\" onerror=\"window.external.notify('img_onerror');showImgDelay(this,'{1}?rnd=123',3);\" style=\"max-width: 250px;\"/></p></a>", imgRealUrl, imgUrl));
                            }

                            // 处理无超链接的图片
                            MatchCollection mcRawImage = Regex.Matches(data, "<img .*?src=\"(?<src>.*?)\".*?/>");
                            foreach (Match m in mcRawImage)
                            {
                                if (!m.Value.Contains("max-width:"))
                                {
                                    string imgUrl = m.Groups["src"].Value;
                                    if (!string.IsNullOrWhiteSpace(imgUrl))
                                    {
                                        string imgTag = string.Format("<img src=\"{0}\" style=\"max-width: 250px;\"", imgUrl);
                                        data = data.Replace(m.Value, imgTag);
                                    }
                                }
                            }

                            // 处理无效链接
                            Regex regexNoLink = new Regex("<a href=[^>]*?（页面不存在）\">(?<title>.*?)</a>");
                            MatchCollection mcNoLink = regexNoLink.Matches(data);
                            foreach (Match m in mcNoLink)
                                data = data.Replace(m.Value, m.Groups["title"].Value);

                            // 处理http链接
                            //Regex regexHttpLink = new Regex("<a .*?>(?<title>http://.*?)</a>");
                            //MatchCollection mcHttpLink = regexHttpLink.Matches(data);
                            //foreach (Match m in mcHttpLink)
                            //    data = data.Replace(m.Groups["title"].Value, "点此导航");

                            // 处理td格式
                            Regex regexTdFormat = new Regex("<td style=\".*?>");
                            MatchCollection mcTdFormat = regexTdFormat.Matches(data);
                            foreach (Match m in mcTdFormat)
                                data = data.Replace(m.Value, "<td>");

                            Regex regexSpoiler = new Regex("<table class=\"common-box\" style=\"margin: 0px 10%; width:250px; background: #fbfbfb; border-left: 10px solid #009af2;\"[\\s\\S]*?<b>以下内容含有剧透成分.*?</b>[\\s\\S]*?</table>");
                            Match matchSpoiler = regexSpoiler.Match(data);
                            if (matchSpoiler.Success)
                                data = data.Replace(matchSpoiler.Value, "<table><td><b>以下内容含有剧透成分，可能影响观赏作品兴趣，请酌情阅读~♡</b></td></table>");

                            Regex regexExpand = new Regex("class=\"mw-collapsible mw-collapsed wikitable\"[\\s\\S]*?(?<th><th>[\\s\\S]*?</th>)");
                            MatchCollection mcExpand = regexExpand.Matches(data);
                            foreach (Match m in mcExpand)
                                data = data.Replace(m.Groups["th"].Value, string.Empty);

                            Regex regexTopicPath = new Regex("<div id=\"topicpath\"[\\s\\S]*?</div>");
                            MatchCollection mcTopicPath = regexTopicPath.Matches(data);
                            foreach (Match m in mcTopicPath)
                                data = data.Replace(m.Value, string.Empty);

                            Regex regexQuote = new Regex("<div style=\"display: table; margin: auto; background-color:transparent;\">.*?</div></div></div></div></div></div>");
                            Match matchQuote = regexQuote.Match(data);
                            if (matchQuote.Success)
                                data = data.Replace(matchQuote.Value, string.Empty);

                            // 强制固定table宽度
                            Regex regexTable = new Regex("<(?<table>table .*?)>");
                            MatchCollection mcTable = regexTable.Matches(data);
                            List<string> listMatchedTable = new List<string>();
                            foreach (Match m in mcTable)
                            {
                                if (listMatchedTable.Contains(m.Value))
                                    continue;
                                data = data.Replace(m.Groups["table"].Value, m.Groups["table"].Value + " style=\"width:100%;table-layout:fixed;word-break:break-all;word-wrap:break-word;\"");
                                listMatchedTable.Add(m.Value);
                            }

                            // 替换<ruby>节点，不然会导致WebBrowser无法正确换行
                            data = data.Replace("<ruby>", string.Empty).Replace("</ruby>", string.Empty);
                            data = data.Replace("<rp>", "<p>").Replace("</rp>", "</p>");
                            data = data.Replace("<rt>", string.Empty).Replace("</rt>", string.Empty);

                            data += "</body>\n</html>";
                            data = data.Replace("<p><br/>", "<p>").Replace("<pre>", string.Empty).Replace("</pre>", string.Empty).Replace("class=\"external free\"", string.Empty).Replace("</noinclude>", string.Empty);

                            _wbStr = data;

                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                ComputeScrolling();
                                wbDetails.NavigateToString(_wbStr);
                                //SystemTray.SetProgressIndicator(this, null);*/
                            });
                        }
                        else
                        {
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                /*ProgressIndicator pi = new ProgressIndicator();
                                pi.Text = "页面无法解析";
                                pi.IsIndeterminate = false;
                                pi.IsVisible = true;
                                SystemTray.SetProgressIndicator(this, pi);*/
                                Windows.System.Launcher.LaunchUriAsync(response.ResponseUri);
                            });
                        }
                    }
                    else
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetWebpageCallback()出错：" + ex.Message);
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    /*ProgressIndicator pi = new ProgressIndicator();
                    if (ex.Message.Contains("NotFound"))
                        pi.Text = "未找到该页面";
                    else
                        pi.Text = "页面加载时出现异常";
                    pi.IsIndeterminate = false;
                    pi.IsVisible = true;*/
                    //SystemTray.SetProgressIndicator(this, pi);//todo
                });
            }
        }

        private void ComputeScrolling()
        {
            //gridBasic.UpdateLayout();
            double heightMaxBasic, heightActualBasic, heightMaxRelated, heightActualRelated;
            gridBasic.UpdateLayout();
            gridRelated.UpdateLayout();

            /*if (Orientation.ToString().StartsWith("Landscape"))
            {
                heightMaxBasic = svBasic.ActualWidth;
                heightActualBasic = gridBasic.ActualWidth;
                heightMaxRelated = svRelated.ActualWidth;
                heightActualRelated = gridRelated.ActualWidth;
            }
            else
            {
                heightMaxBasic = svBasic.ActualHeight;
                heightActualBasic = gridBasic.ActualHeight;
                heightMaxRelated = svRelated.ActualHeight;
                heightActualRelated = gridRelated.ActualHeight;
            }

            Debug.WriteLine("heightMaxBasic=" + heightMaxBasic + ", heightActualBasic=" + heightActualBasic);
            if (heightActualBasic >= heightMaxBasic)
                svBasic.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            gridRelated.UpdateLayout();

            Debug.WriteLine("heightMaxRelated=" + heightMaxRelated + ", heightActualRelated=" + heightActualRelated);
            if (heightActualRelated >= heightMaxRelated)
                svRelated.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;*/
        }

        private void hb_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hyperlinkButton = sender as HyperlinkButton;
            if (hyperlinkButton != null)
            {
                string href = hyperlinkButton.DataContext as string;
                if (!string.IsNullOrWhiteSpace(href))
                {
                    if (href.StartsWith("/"))
                        href = href.Substring(1);
                    string destination = "?href=" + href;
                    //SystemTray.SetProgressIndicator(this, null);
                    Dispatcher.RunAsync (Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
                    {
                        try
                        {
                            Frame.Navigate(typeof(EntryPage ),destination);
                        }
                        catch { }
                    });
                }
                else
                {
                    /*ProgressIndicator pi = new ProgressIndicator { Text = "此条目暂时未被创建", IsIndeterminate = false, IsVisible = true };
                    SystemTray.SetProgressIndicator(this, pi);*/
                }
            }
        }

        private void imgCover_ImageOpened(object sender, RoutedEventArgs e)
        {
            tbBasic.Visibility = Visibility.Collapsed;
        }

        private void imgCover_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            tbBasic.Text = "图片加载失败><";
        }

        private void wbDetails_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            //wbDetails.IsHitTestVisible = false;
        }

        private void wbDetails_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            //wbDetails.IsHitTestVisible = true;
        }



        private void menuHome_Click(object sender, EventArgs e)
        {
            try
            {
                /*while (NavigationService.CanGoBack)
                {
                    var enumerator = NavigationService.BackStack.GetEnumerator();
                    enumerator.MoveNext();
                    if (enumerator.Current.Source.OriginalString.StartsWith("/MainPage.xaml"))
                    {
                        NavigationService.GoBack();
                        break;
                    }
                    NavigationService.RemoveBackEntry();
                }*/
            }
            catch { }
        }
        private async void menuSavePic_Clicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_urlCoverImg))
            {
                /*try
                {
                    using (MediaLibrary ml = new MediaLibrary())
                    {
                        ProgressIndicator pi = new ProgressIndicator { Text = "正在保存图片", IsIndeterminate = true, IsVisible = true };
                        SystemTray.SetProgressIndicator(this, pi);

                        Stream streamImg = await new HttpClient().GetStreamAsync(_urlCoverImg);

                        if (streamImg != null)
                        {
                            pi.Text = "图片保存成功";
                            Thread myWorker = new Thread(() =>
                            {
                                if (ml != null)
                                    ml.SavePicture(_title, streamImg);
                            });
                            myWorker.Start();
                        }
                        else
                            pi.Text = "图片保存失败";

                        pi.IsIndeterminate = false;
                        SystemTray.SetProgressIndicator(this, pi);
                    }
                }
                catch
                {
                    ProgressIndicator pi = new ProgressIndicator { Text = "图片保存失败", IsIndeterminate = false, IsVisible = true };
                    SystemTray.SetProgressIndicator(this, pi);
                }*/
            }
        }
        private void menuWebView_Click(object sender, EventArgs e)
        {
            string url = "http://zh.moegirl.org/" + WebUtility.UrlEncode(_href);
            try
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri( url));

            }
            catch { }
        }

        /*private void menuRefresh_Click(object sender, EventArgs e)
        {
            wbDetails.NavigateToString(_wbStr);
        }*/

        private void menuShare_Click(object sender, EventArgs e)
        {
            try
            {
                /*ShareLinkTask slt = new ShareLinkTask();
                string url = "http://zh.moegirl.org/" + WebUtility.UrlEncode(_href);
                slt.LinkUri = new Uri(url, UriKind.Absolute);
                slt.Title = _title;
                slt.Message = string.Format("萌娘百科条目分享：{0}\n——发送自萌娘百科WP8客户端", _title);
                slt.Show();*/
            }
            catch { }
        }

        private void menuSendReport_Click(object sender, EventArgs e)
        {
            /*EmailComposeTask task = new EmailComposeTask { To = "atelier39@outlook.com" };
            task.Subject = "问题报告 from 萌娘百科 v" + yfxsApp.runtime.Version.GetThisAppVersionString();
            task.Body = string.Format("\n\n条目名称: {0}\n\n", _title);
            try
            {
                task.Show();
            }
            catch { }*/
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ApplicationBar.IsVisible = pivot.SelectedIndex == 0;
        }

        private void wbDetails_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            pivot.SelectedIndex = e.Cumulative.Translation.X < 0 ? 2 : 0;
        }
    }
}
