using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WangZheRongYao
{
    /// <summary>
    /// 网络辅助功能帮助类
    /// </summary>
    public class WebHelper
    {
        /// <summary>
        /// 下载网络文件到本地
        /// </summary>
        /// <param name="Url">网络文件URL</param>
        /// <param name="thisAddress">本地文件 加上文件名和后缀</param>
        /// <returns>返回真假</returns>
        public static async Task<bool> DownloadFile(string Url, string thisAddress)
        {
            HttpClient client = new();
            try
            {
                using var stream = await client.GetStreamAsync(Url);
                using var fileStream = new FileStream(thisAddress, FileMode.OpenOrCreate, FileAccess.Write);
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception) { return false; }
            return true;
        }
    }
}
