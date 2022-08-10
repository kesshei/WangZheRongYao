using HtmlAgilityPack;
using Microsoft.Web.WebView2.Core;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WangZheRongYao
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ImagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        static int Port = 9222;
        static Browser Browser1;
        static PuppeteerSharp.Page Currentpage;
        public MainWindow()
        {
            InitializeComponent();
            webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
            Initialize();
            if (!System.IO.Directory.Exists(ImagesPath))
            {
                System.IO.Directory.CreateDirectory(ImagesPath);
            }
        }

        private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// WebView2初始化
        /// </summary>
        async void Initialize()
        {
            var result = await CoreWebView2Environment.CreateAsync(null, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"),
                new CoreWebView2EnvironmentOptions($"--remote-debugging-port={Port}"));
            await webView2.EnsureCoreWebView2Async(result);
            webView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }
        private async void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            Browser1 = await PuppeteerSharpHelper.GetBrowser(Port, (int)this.Height, (int)this.Width);
            var pages = await Browser1.PagesAsync();
            Currentpage = pages.First();
            await Currentpage.GoToAsync("https://pvp.qq.com/", WaitUntilNavigation.DOMContentLoaded);
            loginfo.Content = "准备就绪";
        }

        private async void start_Click(object sender, RoutedEventArgs e)
        {
            var herolistPath = await Currentpage.EvaluateExpressionAsync<string>("document.querySelector('body > div.wrapper > div.main > div:nth-child(3) > div.skin_center.fl > div.item_header > a').href");

            await Currentpage.GoToAsync(herolistPath, WaitUntilNavigation.DOMContentLoaded);
            loginfo.Content = "开始获取内容";
            var herolist = await Currentpage.EvaluateExpressionAsync<string>("document.querySelector('body > div.wrapper > div > div > div.herolist-box > div.herolist-content > ul').innerHTML");
            var heros = GetHeroInfos(herolist);
            loginfo.Content = $"获取全部英雄信息共:{heros.Count}条";
            foreach (var item in heros)
            {
                await Currentpage.GoToAsync(item.TargetUrl(), WaitUntilNavigation.DOMContentLoaded);
                Thread.Sleep(100);
                var skins = await Currentpage.EvaluateExpressionAsync<string>("document.querySelector('body > div.wrapper > div.zk-con1.zk-con > div > div > div.pic-pf > ul').innerHTML");
                item.HeroSkins = GetHeroSkins(skins);
            }
            loginfo.Content = "开始下载资源";
            var count = 0;
            //开始执行下载
            foreach (var item in heros)
            {
                count++;
                loginfo.Content = $"资源一共:{heros.Count}条，正在下载第{count}条，还剩下:{heros.Count - count}";
                var HearoPath = System.IO.Path.Combine(ImagesPath, item.Name);
                if (!System.IO.Directory.Exists(HearoPath))
                {
                    System.IO.Directory.CreateDirectory(HearoPath);
                }
                foreach (var skin in item.HeroSkins)
                {
                   await WebHelper.DownloadFile(skin.Url, System.IO.Path.Combine(HearoPath, $"{skin.Name}.jpg"));
                }
            }
            loginfo.Content = "采集完毕，等待查看!";
        }
        private List<HeroInfo> GetHeroInfos(string herolist)
        {
            var heroinfos = new List<HeroInfo>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(herolist);

            foreach (var item in doc.DocumentNode.SelectNodes("/li"))
            {
                var heroName = item.InnerText;
                var heroPages = item.LastChild.GetAttributeValue("href", null);
                heroinfos.Add(new HeroInfo() { Name = heroName, Url = heroPages });
            }
            return heroinfos;
        }
        private List<HeroSkin> GetHeroSkins(string skins)
        {
            var HeroSkins = new List<HeroSkin>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(skins);

            foreach (var item in doc.DocumentNode.SelectNodes("/li"))
            {
                var image = item.FirstChild.LastChild;
                var dataimg = image.GetAttributeValue("data-imgname", null);
                var datatitle = image.GetAttributeValue("data-title", null);
                HeroSkins.Add(new HeroSkin(datatitle, dataimg));
            }
            return HeroSkins;
        }
    }
}
