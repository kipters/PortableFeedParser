using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using FeedParser;

namespace FeedParsing
{
    class Program
    {
        static void Main(string[] args)
        {
            var feed = File.Open("feed.xml", FileMode.Open);

            var parser = new FeedParserService();

            var channel = parser.ParseRss2Async(feed).Result;

            Debugger.Break();

            /*var xdoc = XDocument.Load(feed);
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            var list =
                xdoc.Descendants("channel")
                    .FirstOrDefault()
                    .Descendants("item")
                    .FirstOrDefault()
                    .Descendants(dc + "creator")
                    .FirstOrDefault()
                    .Value;

            var cleanArray = new char[list.Length];
            int cleanIndex = 0;
            bool skipping = false;
            for (int i = 0; i < list.Length; i++)
            {
                var current = list[i];
                if (current != '<' && !skipping)
                {
                    cleanArray[cleanIndex++] = current;
                    continue;
                }
                else if (current == '>' && skipping)
                {
                    skipping = false;
                }
                else
                {
                    skipping = true;
                }
            }
            cleanArray[cleanIndex++] = '\0';
            var cleanString = new string(cleanArray);
            var unencoded = HttpUtility.HtmlDecode(list).Trim();
            Console.WriteLine(unencoded.Trim());*/
            Console.ReadLine();
            Debugger.Break();
        }
    }
}
