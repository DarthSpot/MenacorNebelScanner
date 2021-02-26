using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;

namespace MenacorNebelScanner
{
    public class MenacorScanner
    {
        private static string url = "https://dsaforum.de";
        private static string viewForum = "viewforum.php";
        private static string viewThread = "viewtopic.php";
        private WebClient client = new WebClient();
        private int parses = 8;

        public delegate void UpdateProgressHandler(object sender, int counter);

        public event UpdateProgressHandler OnUpdateProgress;

        private void UpdateProgress(int count)
        {
            OnUpdateProgress?.Invoke(this, count);
        }
        
        public MenacorScanner()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            client.Encoding = Encoding.UTF8;
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4");
            client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
        }

        public List<Post> Scan(int forumId, int threadId)
        {
            return ScanPage(forumId, threadId, null, 1);
        }

        private List<Post> ScanPage(int forum, int thread, int? start, int pageCounter)
        {
            int? next = null;
            var res = new List<Post>();
            PerformRetryAction(() =>
            {
                var threadUrl = start != null
                    ? $"{url}/{viewThread}?f={forum}&t={thread}&start={start}"
                    : $"{url}/{viewThread}?f={forum}&t={thread}";
                var doc = new HtmlDocument();
                doc.LoadHtml(client.DownloadString(threadUrl));
                // Specifc error in one single FAB. Page not loading
                if (!doc.DocumentNode.Descendants("head").Any())
                {
                    next = start + 30;
                }
                else
                {
                    UpdateProgress(pageCounter);
                    var threadName = doc.DocumentNode.FindByClass("h2", "topic-title")?.InnerText;
                    var nextbutton = doc.DocumentNode.FindByClass("div", "action-bar bar-top").FindByClass("li", "arrow next");
                    if (nextbutton != null)
                    {
                        next = Convert.ToInt32(Regex.Match(nextbutton.Element("a")?.AttributeValue("href"), ".*start=(?<start>[0-9]+)").Groups["start"].Value);
                    }

                    foreach (var container in doc.DocumentNode.Descendants("div").Where(x =>
                        string.Equals(x.GetAttributeValue("class", ""), ("post-container"))))
                    {
                        var post = container.Elements("div").First();
                        var id = post.GetAttributeValue("id", "");
                        var profile = post.FindByClass("dl", "postprofile");
                        var body = post.FindByClass("div", "postbody");
                        var timeStampText = body.FindByClass("p", "author").LastChild.InnerText;
                        var timeStamp = DateTime.Parse(timeStampText);
                        var name = profile.Find(x => x.AttributeValue("class").StartsWith("username")).InnerText;
                        var length = body.FindByClass("div", "content").InnerText.Length;

                        var postItem = new Post(id, timeStamp);
                        postItem.User = name;
                        postItem.PostLength = length;
                        res.Add(postItem);
                    }
                }

            });

            if (next != null)
                res.AddRange(ScanPage(forum, thread, next, pageCounter + 1));

            return res;
        }
        
        private void PerformRetryAction(Action action)
        {
            var parse = 0;
            var finished = false;
            while (!finished && parse < parses)
            {
                try
                {
                    action();
                    finished = true;
                }
                catch (Exception ex)
                {
                    parse++;
                    Thread.Sleep(1000);
                }
            }
            if (!finished)
                throw new Exception("Something went wrong...");
        }
    }

    public class Post
    {
        public string Id { get; }
        public DateTime TimeStamp { get; }
        public string User { get; set; }
        public int PostLength { get; set; }

        public Post(string id, DateTime timeStamp)
        {
            Id = id;
            TimeStamp = timeStamp;
        }

        public override string ToString()
        {
            return $"{Id};{User};{TimeStamp:dd.MM.yyyy-hh:mm:ss};{PostLength}";
        }
    }

    public static class HtmlNodeExtension
    {
        public static HtmlNode FindByClass(this HtmlNode node, string name, string className)
        {
            return node.Descendants(name).SingleOrDefault(x => string.Equals(x.AttributeValue("class"), className, StringComparison.Ordinal));
        }

        public static HtmlNode FindByClass(this HtmlNode node, string className)
        {
            return node.Descendants().SingleOrDefault(x => string.Equals(x.AttributeValue("class"), className, StringComparison.Ordinal));
        }

        public static HtmlNode Find(this HtmlNode node, string name, Func<HtmlNode, bool> filter)
        {
            return node.Descendants(name).SingleOrDefault(filter);
        }

        public static HtmlNode Find(this HtmlNode node, Func<HtmlNode, bool> filter)
        {
            return node.Descendants().SingleOrDefault(filter);
        }


        public static string AttributeValue(this HtmlNode node, string attributeName)
        {
            return node.GetAttributeValue(attributeName, "");
        }
    }

    public class MenacorData
    {
    }
}