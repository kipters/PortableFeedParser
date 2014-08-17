using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FeedParser
{
    public class FeedItem
    {
        public FeedItem(XElement xElement, int cropping = 50)
        {
            var x = xElement;
            
            var titleElement = x.Descendants("title").FirstOrDefault();
            if (titleElement != null) Title = titleElement.Value;

            var linkElement = x.Descendants("link").FirstOrDefault();
            if (linkElement != null) Link = linkElement.Value;

            var commentsElement = x.Descendants("comments").FirstOrDefault();
            if (commentsElement != null) Comments = commentsElement.Value;

            var dateElement = x.Descendants("pubDate").FirstOrDefault();
            if (dateElement != null) 
                PubDate = DateTime.Parse(dateElement.Value);
            else
                PubDate = null;

            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            var creatorElements = x.Descendants(dc + "creator");
            var creatorElement = creatorElements.FirstOrDefault();
            if (creatorElement != null) Creator = creatorElement.Value;

            var descriptionElement = x.Descendants("description").FirstOrDefault();
            if (descriptionElement != null)
            {
                //var description = descriptionElement.Value.Substring(0, 50);
                var cleanString = descriptionElement.Value.StripHtmlTags(50);
                Description = cleanString;
            }

            XNamespace content = "http://purl.org/rss/1.0/modules/content/";
            var contentElements = x.Descendants(content + "encoded");
            var contentElement = contentElements.FirstOrDefault();
            if (contentElement != null) Content = contentElement.Value;
        }

        public string Title { get; private set; }
        public string Link { get; private set; }
        public string Comments { get; private set; }
        public DateTime? PubDate { get; private set; }
        public string Creator { get; private set; }
        public string Description { get; private set; }
        public string Content { get; private set; }
    }

    public static class Extensions
    {
        public static string StripHtmlTags(this string toClean, int crop)
        {
            var cleanArray = new char[toClean.Length];
            int index = 0;
            bool skipping = false;
            int cropLen = 0;

            if (crop < 0)
                cropLen = cleanArray.Length;
            else
                cropLen = Math.Min(crop, cleanArray.Length);

            for (int i = 0; i < cropLen; i++)
            {
                var c = toClean[i];
                if (c == '<')
                {
                    skipping = true;
                    continue;
                }
                if (c == '>')
                {
                    skipping = false;
                    continue;
                }
                if (!skipping)
                {
                    cleanArray[index++] = c;
                }
            }
            for (int i = 0; i < 3; i++)
                cleanArray[index++] = '.';
            var cleanString = new string(cleanArray, 0, index);
            cleanString = cleanString.Trim();
            return cleanString;
        }
    }

    public class FeedChannel
    {
        public FeedChannel(string title, string link, DateTime? pubDate, IEnumerable<FeedItem> items)
        {
            Title = title;
            Link = link;
            LastBuild = pubDate;
            FeedItems = items.ToList();
        }
        public string Title { get; private set; }
        public string Link { get; private set; }
        public DateTime? LastBuild { get; private set; }
        public IEnumerable<FeedItem> FeedItems { get; private set; }
    }

    public interface IFeedParser
    {
        Task<FeedChannel> ParseRss2Async(Stream feed);
        Task<FeedChannel> ParseRss2Async(Uri uri);
        Task<FeedChannel> ParseRss2Async(string content);
        int DescriptionCropping { get; set; }
    }

    public class FeedParserService : IFeedParser
    {
        public FeedParserService(int cropping)
        {
            DescriptionCropping = cropping;
        }

        public FeedParserService() : this(-1)
        {
        }

        public Task<FeedChannel> ParseRss2Async(Stream feed)
        {
            var xdoc = XDocument.Load(feed);
            return ParseRss2Async(xdoc);
        }

        public async Task<FeedChannel> ParseRss2Async(Uri uri)
        {
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(uri);
            var result = await ParseRss2Async(stream);
            return result;
        }

        public Task<FeedChannel> ParseRss2Async(string content)
        {
            var xdoc = XDocument.Parse(content);
            return ParseRss2Async(xdoc);
        }

        private Task<FeedChannel> ParseRss2Async(XDocument xdoc)
        {
            return Task.Run(() =>
            {
                var channel = xdoc.Descendants("channel").FirstOrDefault();
                if (channel == null) return null;
                string title = "", link = "";
                var titleElement = channel.Descendants("title").FirstOrDefault();
                if (titleElement != null) title = titleElement.Value;
                var linkElement = channel.Descendants("link").FirstOrDefault();
                if (linkElement != null) link = linkElement.Value;
                DateTime? pubDate = null;
                var pubDateElement = channel.Descendants("lastBuildDate").FirstOrDefault();
                if (pubDateElement != null) pubDate = DateTime.Parse(pubDateElement.Value);

                var rawItems = channel.Descendants("item");
                var items = rawItems.Select(x => new FeedItem(x)).AsEnumerable();

                var result = new FeedChannel(title, link, pubDate, items);
                return result;
            });
        }

        public int DescriptionCropping { get; set; }
    }
}
