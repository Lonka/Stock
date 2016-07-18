using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Stock
{
    public static class StockHelper
    {
        public static double GetQuotedMarketPrice(string stockCode)
        {

            // 使用預設編碼讀入 HTML 
            HtmlDocument doc = GetHtemlContent("https://tw.stock.yahoo.com/q/q?s=" + stockCode);

            // 裝載第一層查詢結果 
            HtmlDocument hdc = new HtmlDocument();

            //XPath 來解讀它 /html[1]/body[1]/center[1]/table[2]/tr[1]/td[1]/table[1] 
            hdc.LoadHtml(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[2]/tr[1]/td[1]/table[1]").InnerHtml);

            // 取得個股標頭 
            HtmlNodeCollection htnode = hdc.DocumentNode.SelectNodes("./tr[1]/th");
            // 取得個股數值 
            string[] txt = hdc.DocumentNode.SelectSingleNode("./tr[2]").InnerText.Trim().Split('\n');

            doc = null;
            hdc = null;
            double result = -1;
            if (!double.TryParse(txt[2].Trim(),out result))
            {
                result = -1;
            }
            return result;
        }


        public static double GetOpenPrice(string stockCode)
        {

            // 使用預設編碼讀入 HTML 
            HtmlDocument doc = GetHtemlContent("https://tw.stock.yahoo.com/q/q?s=" + stockCode);

            // 裝載第一層查詢結果 
            HtmlDocument hdc = new HtmlDocument();

            //XPath 來解讀它 /html[1]/body[1]/center[1]/table[2]/tr[1]/td[1]/table[1] 
            hdc.LoadHtml(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[2]/tr[1]/td[1]/table[1]").InnerHtml);

            // 取得個股標頭 
            HtmlNodeCollection htnode = hdc.DocumentNode.SelectNodes("./tr[1]/th");
            // 取得個股數值 
            string[] txt = hdc.DocumentNode.SelectSingleNode("./tr[2]").InnerText.Trim().Split('\n');

            doc = null;
            hdc = null;
            double result = -1;
            if (!double.TryParse(txt[8].Trim(), out result))
            {
                result = -1;
            }
            return result;
        }

        public static HtmlDocument GetHtemlContent(string url)
        {
            //url = "https://tw.stock.yahoo.com/q/q?s=2308";
            HtmlDocument content = new HtmlDocument();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebProxy proxy = new WebProxy();
            proxy.UseDefaultCredentials = true;
            request.Proxy = proxy;
            request.Timeout = 5000;
            using (HttpWebResponse reponse = (HttpWebResponse)request.GetResponse())
            {
                string coder = ((HttpWebResponse)reponse).CharacterSet;
                Stream streamReceive = reponse.GetResponseStream();
                content.Load(streamReceive, Encoding.GetEncoding(coder));
            }
            return content;
        }
    }
}
