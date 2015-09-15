﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace SearchBackend
{
    public class Crawler
    {
        private string _start;
        private Dictionary<string, int> seenURLS;
        private Logger logger;
        public Crawler(string StartURL)
        {
            _start = StartURL;
            seenURLS = new Dictionary<string, int>();
            logger = new Logger("crawler.txt", 30);
        }

        // Starts at the starting URL and continues crawling links and updating database
        public void Crawl()
        {
            Queue<string> WebBFS = new Queue<string>();
            logger.Initialize();
            WebBFS.Enqueue(_start);

            while(true)
            {
                try
                {
                    string currentURL;

                    lock (WebBFS)
                    {
                        currentURL = WebBFS.Dequeue();
                    }

                    Console.WriteLine("Crawling " + currentURL);
                    WebRequest request = WebRequest.Create(currentURL);

                    IAsyncResult result = request.BeginGetResponse((IAsyncResult v) => 
                    {
                        try
                        {
                            WebResponse response = request.EndGetResponse(v);
                            Stream data = response.GetResponseStream();
                            string html = String.Empty;
                            using (StreamReader sr = new StreamReader(data))
                            {
                                html = sr.ReadToEnd();
                            }

                            html = SanitizeHtml(html);

                            IEnumerable<string> urls = GetURLS(html);

                            // Enqueue all the sites found from links
                            foreach (string s in urls)
                            {
                                string nextURL = s;

                                // If the link is relative add the current url
                                if (s.StartsWith("/"))
                                {
                                    nextURL = currentURL + s;
                                }
                                else if(!s.StartsWith("http"))
                                {
                                    nextURL = currentURL + "/" + s;
                                }
                                lock (WebBFS)
                                {
                                    // Check if URL exists in our set
                                    if(seenURLS.ContainsKey(nextURL))
                                    {
                                        seenURLS[nextURL]++;
                                    }
                                    else
                                    {
                                        seenURLS.Add(nextURL, 1);
                                        WebBFS.Enqueue(nextURL);
                                    }
                                }
                                //Console.WriteLine(nextURL);
                            }
                        }
                        catch(WebException we)
                        {
                            // Log failure
                            logger.Log(DateTime.Now + " " + we.Message + " " + "(" + currentURL + ")" + Environment.NewLine);
                                
                        }
                        catch(Exception e)
                        {
                            return;
                        }

                    }, null);
                }
                catch(Exception e)
                {
                    continue;
                }
            }
        }
        private static IEnumerable<string> GetURLS(string html)
        {
            List<string> list = new List<String>();

            var reg = new Regex("(<a|link)[^>]* href=\"([^\"]*)");

            foreach (Match match in reg.Matches(html))
            {
                string newurl = match.Groups[2].ToString();
                if (newurl.EndsWith("/"))
                {
                    newurl = newurl.Remove(newurl.Length - 1);
                }

                list.Add(newurl);
            }

            return list;

        }

        // Remove unwanted data from the HTML
        private static string SanitizeHtml(string html)
        {
            // Remove all Javascript and CSS
            var regex = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = regex.Replace(html, "");

            string acceptable = "link|a href|a";
            string stringPattern = @"</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
            return Regex.Replace(html, stringPattern, "");
        }
    }
}
