using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WangZheRongYao
{
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
}
