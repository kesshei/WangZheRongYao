using System;
using System.Collections.Generic;
using System.Text;

namespace WangZheRongYao
{
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
}
