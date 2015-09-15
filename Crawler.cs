using System;
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
        private Dictionary<string, int> _seenURLS;
        private Logger _logger;
        public Crawler(string StartURL)
        {
            _start = StartURL;
            _seenURLS = new Dictionary<string, int>();
            _logger = new Logger("crawler.txt", 30);
        }

        // Starts at the starting URL and continues crawling links and updating database
        public void Crawl()
        {
            Queue<string> WebBFS = new Queue<string>();
            _logger.Initialize();
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
                            Dictionary<string, bool> localSeen = new Dictionary<string, bool>();

                            using (StreamReader sr = new StreamReader(data))
                            {
                                html = sr.ReadToEnd();
                            }

                            html = Parser.SanitizeHtml(html);

                            IEnumerable<string> urls = Parser.GetURLS(html);

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
                                    // Don't allow a site to incrase the page rank of a url more than once
                                    if (localSeen.ContainsKey(nextURL))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        localSeen.Add(nextURL, true);
                                    }
                                    // Check if URL exists in our set
                                    if(_seenURLS.ContainsKey(nextURL))
                                    {
                                        _seenURLS[nextURL]++;
                                    }
                                    else
                                    {
                                        _seenURLS.Add(nextURL, 1);
                                        WebBFS.Enqueue(nextURL);
                                    }
                                }
                                //Console.WriteLine(nextURL);
                            }
                        }
                        catch(WebException we)
                        {
                            // Log failure
                            _logger.Log(DateTime.Now + " " + we.Message + " " + "(" + currentURL + ")" + Environment.NewLine);
                                
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
    }
}
