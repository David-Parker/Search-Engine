using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchBackend
{
    // Parser class is responsible for getting the data out of the HTML
    public static class Parser
    {
        public static IEnumerable<string> GetURLS(string html)
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
        public static string SanitizeHtml(string html)
        {
            // Remove all Javascript and CSS
            var regex = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = regex.Replace(html, "");

            string acceptable = "link|a href|a";
            string stringPattern = @"</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
            return Regex.Replace(html, stringPattern, "");
        }

        // This method scans through the HTML and returns all the plain text no html tags
        public static string GetContent(string HTML)
        {
            return null;
        }

        // This method runs through the content and builds a list of known keywords and their counts
        public static Dictionary<string, int> GetKeywords(string HTML)
        {
            return null;
        }
    }
}
