using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;

namespace Linqsxom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Content-Type: text/html; charset=UTF-8\r\n\r\n" +
                            (from i in Enumerable.Range(0, 1)
                             let maxRecentCount = 5
                             let dir = @"Data" //@"..\..\Data"

                             let getString = (Func<String, String>)(s => String.IsNullOrEmpty(s) ? "" : s)

                             let serverName = Environment.GetEnvironmentVariable("SERVER_NAME")
                             let serverPort = Environment.GetEnvironmentVariable("SERVER_PORT")
                             let serverUrlPrefix = String.IsNullOrEmpty(serverName) ? "" : String.Format("http://{0}{1}", serverName, (serverPort == "80" ? "" : ":" + serverPort))
                             let scriptUrl = serverUrlPrefix + Environment.GetEnvironmentVariable("SCRIPT_NAME")

                             let pathInfo = getString(Environment.GetEnvironmentVariable("PATH_INFO")) /* "/2007/11" */

                             let pathInfoExtension = Path.GetExtension(pathInfo)
                             let flavour = String.IsNullOrEmpty(pathInfoExtension) ? "html" : pathInfoExtension.Substring(1)
                             let headTemplate = File.Exists("head."+flavour) ? File.ReadAllText("head."+flavour) : "<!DOCTYPE html>\n<title>Linqsxom</title>\n<h1>Linqsxom</h1>\n"
                             let bodyTemplate = File.Exists("body."+flavour) ? File.ReadAllText("body."+flavour) : "<article>\n<h1><a href=\"{ScriptUrl}{Path}\">{Title}</a></h1>\n<p><time>{CreatedOn}</time></p>\n<div class=\"body\">\n{Body}\n</div>\n</article>\n"
                             let footTemplate = File.Exists("foot."+flavour) ? File.ReadAllText("foot."+flavour) : "<p>Genetated by Linqsxom</p>"

                             let body = String.Join("\n", (from entry in
                                                               (from f in
                                                                    (from f in Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories)
                                                                     orderby File.GetLastWriteTime(f) descending
                                                                     select f).Take(maxRecentCount)
                                                                let createdOn = File.GetCreationTime(f)
                                                                let textContent = File.ReadAllLines(f)
                                                                select new
                                                                {
                                                                    Path = String.Format("/{0}/{1:00}/{2:00}/{3}", createdOn.Year, createdOn.Month, createdOn.Day, Path.GetFileNameWithoutExtension(f) + ".html"),
                                                                    FileName = f,
                                                                    CreatedOn = createdOn,
                                                                    Title = textContent.FirstOrDefault() ?? "",
                                                                    Body = String.Join("", textContent.Skip(1).ToArray())
                                                                })
                                                           where entry.Path == pathInfo || entry.Path.StartsWith(pathInfo + "/") || String.IsNullOrEmpty(pathInfo)
                                                           let block = bodyTemplate.Replace(CreatedOn => entry.CreatedOn.ToString(),
                                                                                            Title     => entry.Title,
                                                                                            Body      => entry.Body,
                                                                                            ScriptUrl => scriptUrl,
                                                                                            Path      => entry.Path)
                                                           select block).ToArray<String>())
                             select headTemplate + body + footTemplate).First().ToString());
            //Console.Write("Content-Type: text/html; charset=UTF-8\r\n\r\n"+t.First().ToString());
        }
    }

    static class Extensions
    {
        /// <example>
        /// var s = "{Foo}/{Gaogao}".Replace(Foo => "ふう。", Gaogao => "がおがお。"); // => "ふう。/がおがお。"
        /// </example>
        public static String Replace(this String s, params Expression<Func<Object, String>>[] exprs)
        {
            return (from i in Enumerable.Range(0, 1)
                    let sb = ((Func<String, StringBuilder>)(x => new StringBuilder(x)))(s)
                    let body = (from expr in exprs
                                where sb.Replace("{" + expr.Parameters[0].Name + "}", expr.Compile().Invoke(s)) != null
                                select sb).Last()
                    select body).First().ToString();
        }
    }
}
