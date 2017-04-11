using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IdentityGuesser.Models
{
    class BingWebSearcher
    {
        //https://api.cognitive.microsoft.com/bing/v5.0/images/search
        // original: https://api.cognitive.microsoft.com/bing/v5.0/search?q={0}&count=5&offset=0&mkt=en-us&safesearch=Moderate

        private static string template = @"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={0}&count=5";
        private static HtmlWeb web = new HtmlWeb();

        //Ocp-Apim-Subscription-Key
        // mine: 0f6147d5a4c044549e312a1560eaa209
        // their: 1763fff52c254801a175674b061e0b34
        private static string SubscriptionKey = "0f6147d5a4c044549e312a1560eaa209";

        /// <summary>
        /// This function returns the textual (not the link) representation of the search
        /// the returned texts are the "name" part of the searching result
        /// </summary>
        /// <param name="queryFilePath">The keyword to search</param>
        /// <returns>A list of searching result texts that relates to the queryFilePath text.</returns>
        public static List<SearchResult> Search(string queryFilePath)
        {
            var json = GetBingSearchJsonResult(queryFilePath);
            var result = JsonConvert.DeserializeObject<Rootobject>(json); // values are here 

            return result?.value.Select(item => new SearchResult
            {
                Name = item.name,
                Link = item.thumbnailUrl
            }).ToList();
        }


        /// <summary>
        /// This function gets the json response from bing search
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private static string GetBingSearchJsonResult(string keywords)
        {
            string JsonString = null;
            string url = string.Format(template, keywords);

            using (var client = new WebClient())
            {
                //client.Headers[HttpRequestHeader.UserAgent] = "some user agent if you wish";

                client.Headers[HttpRequestHeader.AcceptLanguage] = "es-ES";
                client.Headers["Ocp-Apim-Subscription-Key"] = SubscriptionKey;
                string html = client.DownloadString(url);

                // feed the HTML to HTML Agility Pack
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                JsonString = doc.DocumentNode.InnerText;
                return JsonString;
            }
        }
    }
}

public class SearchResult
{
    public string Name { get; set; }
    public string Link { get; set; }
}


/// <summary>
/// Embeded classes for serialization of Bing search results json string
/// </summary>
public class Rootobject
{
    public string _type { get; set; }
    public Instrumentation instrumentation { get; set; }
    public string readLink { get; set; }
    public string webSearchUrl { get; set; }
    public int totalEstimatedMatches { get; set; }
    public List<Value> value { get; set; }
}

public class Instrumentation
{
    public string pageLoadPingUrl { get; set; }
}

public class Webpages
{
    public string webSearchUrl { get; set; }
    public int totalEstimatedMatches { get; set; }
    public Value[] value { get; set; }
}

public class Value
{
    public string name { get; set; }
    public string webSearchUrl { get; set; }
    public string thumbnailUrl { get; set; }
    public string datePublished { get; set; }
    public string contentUrl { get; set; }
    public string hostPageUrl { get; set; }
    public string contentSize { get; set; }
    public string encodingFormat { get; set; }
    public string hostPageDisplayUrl { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class Rankingresponse
{
    public Mainline mainline { get; set; }
}

public class Mainline
{
    public Item[] items { get; set; }
}

public class Item
{
    public string answerType { get; set; }
    public int resultIndex { get; set; }
    public Value1 value { get; set; }
}

public class Value1
{
    public string id { get; set; }
}