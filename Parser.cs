using HtmlAgilityPack;
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
        public static Regex contentRegex = new Regex(@"<([^>""]*|""[^""]*"")*>", RegexOptions.Compiled);

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

                // Remove query strings
                newurl = Regex.Replace(newurl, "[?&].*", "");

                list.Add(newurl);
            }

            return list;
        }

        // Remove unwanted data from the HTML
        public static string SanitizeHtml(string html)
        {
            // Remove all Javascript and CSS
            var regex = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)|(<!--.*?-->)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            string acceptable = "link|a href|a";
            string myregex = "(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)|(<!--.*?-->)|(</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>)";
            return Regex.Replace(html, myregex, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        }

        // This method scans through the HTML and returns all the plain text no html tags
        public static string GetContent(string HTML)
            
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(HTML);

            var root = doc.DocumentNode;
            var sb = new StringBuilder();
            foreach (var node in root.DescendantsAndSelf())
            {
                if (!node.HasChildNodes)
                {
                    string text = node.InnerText;
                    if (!string.IsNullOrEmpty(text))
                        sb.AppendLine(text.Trim());
                }
            }

            string ret = sb.ToString();

            //Console.WriteLine(HTML);

            //string ret = contentRegex.Replace(HTML, "");

            //Console.WriteLine(ret);
            // Remove non alpha characters
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            ret = rgx.Replace(ret, "");

            return ret;
        }

        // This method runs through the content and builds a list of known keywords and their counts
        public static Dictionary<string, int> GetKeywords(string HTML)
        {
            Dictionary<string, bool> blacklist = new Dictionary<string, bool>()
            {
                {"a", true},
                {"aboard", true},
                {"about", true},
                {"above", true},
                {"across", true},
                {"after", true},
                {"against", true},
                {"all", true},
                {"along", true},
                {"although", true},
                {"amid", true},
                {"among", true},
                {"and", true},
                {"another", true},
                {"anti", true},
                {"any", true},
                {"anybody", true},
                {"anyone", true},
                {"anything", true},
                {"around", true},
                {"as", true},
                {"assuming", true},
                {"at", true},
                {"be", true},
                {"because", true},
                {"been", true},
                {"before", true},
                {"behind", true},
                {"being", true},
                {"below", true},
                {"beneath", true},
                {"beside", true},
                {"besides", true},
                {"between", true},
                {"beyond", true},
                {"both", true},
                {"but", true},
                {"by", true},
                {"concerning", true},
                {"considering", true},
                {"despite", true},
                {"down", true},
                {"during", true},
                {"each", true},
                {"either", true},
                {"even", true},
                {"everybody", true},
                {"everyone", true},
                {"everything", true},
                {"except", true},
                {"excepting", true},
                {"excluding", true},
                {"few", true},
                {"following", true},
                {"for", true},
                {"from", true},
                {"he", true},
                {"her", true},
                {"hers", true},
                {"herself", true},
                {"him", true},
                {"himself", true},
                {"his", true},
                {"how", true},
                {"I", true},
                {"if", true},
                {"in", true},
                {"inside", true},
                {"into", true},
                {"is", true},
                {"it", true},
                {"its", true},
                {"itself", true},
                {"lest", true},
                {"like", true},
                {"many", true},
                {"me", true},
                {"mine", true},
                {"minus", true},
                {"more", true},
                {"most", true},
                {"much", true},
                {"my", true},
                {"myself", true},
                {"near", true},
                {"neither", true},
                {"nobody", true},
                {"none", true},
                {"noone", true},
                {"nothing", true},
                {"of", true},
                {"off", true},
                {"on", true},
                {"only", true},
                {"onto", true},
                {"opposite", true},
                {"other", true},
                {"others", true},
                {"our", true},
                {"ours", true},
                {"ourselves", true},
                {"outside", true},
                {"over", true},
                {"past", true},
                {"per", true},
                {"plus", true},
                {"provided", true},
                {"rather", true},
                {"regarding", true},
                {"round", true},
                {"save", true},
                {"several", true},
                {"she", true},
                {"since", true},
                {"so", true},
                {"some", true},
                {"somebody", true},
                {"someone", true},
                {"something", true},
                {"than", true},
                {"that", true},
                {"the", true},
                {"their", true},
                {"theirs", true},
                {"them", true},
                {"themselves", true},
                {"these", true},
                {"they", true},
                {"this", true},
                {"those", true},
                {"though", true},
                {"through", true},
                {"to", true},
                {"toward", true},
                {"towards", true},
                {"under", true},
                {"underneath", true},
                {"unless", true},
                {"unlike", true},
                {"until", true},
                {"up", true},
                {"upon", true},
                {"us", true},
                {"versus", true},
                {"via", true},
                {"was", true},
                {"we", true},
                {"what", true},
                {"whatever", true},
                {"whereas", true},
                {"whether", true},
                {"which", true},
                {"whichever", true},
                {"while", true},
                {"who", true},
                {"whoever", true},
                {"whom", true},
                {"whomever", true},
                {"whose", true},
                {"why", true},
                {"with", true},
                {"within", true},
                {"without", true},
                {"you", true},
                {"your", true},
                {"yours", true},
                {"yourself", true},
                {"yourselves", true}
            };

            Dictionary<string, int> table = new Dictionary<string, int>();

            foreach(string word in HTML.Split(' '))
            {
                if (word == "") continue;

                string wordLower = word.ToLower();
                // Non-blacklisted word
                if (!blacklist.ContainsKey(wordLower))
                {
                    if(table.ContainsKey(wordLower))
                    {
                        table[wordLower]++;
                    }
                    else
                    {
                        table.Add(wordLower, 1);
                    }
                }

            }
            return table;
        }
    }
}
