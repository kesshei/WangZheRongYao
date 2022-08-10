# WebView2 通过 PuppeteerSharp 实现爬取 王者 壁纸 (案例版) 
> 此案例是《.Net WebView2 项目，实现 嵌入 WEB 页面 Chromium内核》文的续集。

>主要是针对WebView2的一些微软自己封装的不熟悉的API，有一些人已经对 PuppeteerSharp很熟悉了，那么，直接用 PuppeteerSharp的话，那就降低了学习成本，那还是很有必须要的。

>之前自己也RPA获取过联盟的高清原画，现在就获取下王者的高清壁纸。

# 王者壁纸自动化获取逻辑分析
其实它的逻辑很简单， 就是王者的官网，打开后，在右下角就看到了皮肤页面部分。

这个时候，点击更多，就会打开全部英雄详情的页面。

这个时候，单点任意一个英雄，就会新开一个页面，这个英雄自己的页面，可以看到具体的皮肤信息了。

这里可以看到有6个皮肤，那么，到这里我就可以获取这6个皮肤作为高清王者的皮肤了。

那么，让程序自动化操作，并把这些信息处理保存好，就是我们要做到的事情。


## 新建一个WPF项目

新建一个 WPF 项目，要添加 Nuget 包
```csharp
Install-Package Microsoft.Web.WebView2 -Version 1.0.1293.44
Install-Package PuppeteerSharp -Version 7.1.0
Install-Package HtmlAgilityPack -Version 1.11.43
```
### MainWindow.xaml
界面大致样子和布局
```csharp
<DockPanel>
    <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" HorizontalAlignment="Right">
        <Label Name = "loginfo" Content="未采集"/>
        <Button Name="start" DockPanel.Dock="Right" Width="150" Content="开始采集" Click="start_Click"/>
    </StackPanel>
    <wpf:WebView2 Name = "webView2"/>
</DockPanel>
```

右上角一个提示信息，一个采集的按钮，布局很是简单

###  如何启用 PuppeteerSharp 
其实都是基于谷歌的DevTools协议来的，所以，只要WebView2开启了Debugging端口即可。
```csharp
var result = await CoreWebView2Environment.CreateAsync(null, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"),
    new CoreWebView2EnvironmentOptions($"--remote-debugging-port={Port}"));
await webView2.EnsureCoreWebView2Async(result);
```
通过WebVeiw2的游览器启动参数 : --remote-debugging-port=6666 来开启DevTools协议的支持。

#### PuppeteerSharpHelper
```csharp
public class PuppeteerSharpHelper
{
    /// <summary>
    /// 获取游览器对象
    /// </summary>
    public static Task<Browser> GetBrowser(int port, int height, int width)
    {
        return Puppeteer.ConnectAsync(new ConnectOptions { DefaultViewport = new ViewPortOptions() { Height = height, Width = width }, BrowserWSEndpoint = WSEndpointResponse.GetWebSocketDebuggerUrl(port) });
    }
    internal class WSEndpointResponse
    {
        public string WebSocketDebuggerUrl { get; set; }
        public static string GetWebSocketDebuggerUrl(int port)
        {
            string data;
            using (var client = new HttpClient())
            {
                data = client.GetStringAsync($"http://127.0.0.1:{port}/json/version").Result;
            }
            return JsonConvert.DeserializeObject<WSEndpointResponse>(data).WebSocketDebuggerUrl;
        }
    }
}
```

### 所用到的王者实体信息
```csharp
/// <summary>
/// 英雄的信息
/// </summary>
public class HeroInfo
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string TargetUrl()
    {
        return $"https://pvp.qq.com/web201605/{Url}";
    }
    public List<HeroSkin> HeroSkins { get; set; }
}
/// <summary>
/// 英雄皮肤
/// </summary>
public class HeroSkin
{
    public HeroSkin(string name, string url)
    {
        this.Name = name;
        this.Url = "https:" + url;
    }
    public string Name { get; set; }
    public string Url { get; set; }
}
```
## RPA的核心代码
```csharp
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
    loginfo.Content = "获取完毕，等待查看!";
}
```
## 效果如下:
需要点击获取按钮，就会执行自动化获取操作，然后把获取的内容存储到当前项目bin目录images目录下。

下面就是下载完后的效果。

![](https://tupian.wanmeisys.com/markdown/1660060883775-f4cb6a1e-ff7c-4af5-afd2-01a2713de08c.png)
整整齐齐，很完整，都是我喜欢的英雄和买不起的皮肤。

![](https://tupian.wanmeisys.com/markdown/1660060903573-1c8b803a-8616-45f4-81fc-d2417eff16e7.png)
而且，获取到的包含了皮肤的名称

## 总结
基于WebView2，技术又深一层次的展开，一个好的技术，必定用到合适的场景上才是最合适的。

## 代码地址
https://github.com/kesshei/WangZheRongYao.git

https://gitee.com/kesshei/WangZheRongYao.git

# 阅

一键三连呦！，感谢大佬的支持，您的支持就是我的动力!

# 版权

蓝创精英团队（公众号同名，CSDN 同名，CNBlogs 同名）

