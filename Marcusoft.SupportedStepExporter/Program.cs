using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow;

namespace Marcusoft.SupportedStepExporter
{
    static class Program
    {
        private const string TITLE_FORMAT = "<html><head><title>{0}</title></head><body>";
        private const string FOOTER = "</body></html>";
        private const string HEADER_FORMAT = "<h1>{0} can handle the following regular expressions</h1>";
        private const string REGEXP_FORMAT = "<li>{0}</li>";
        private const string STEP_FORMAT = "<h2>{0} steps</h2><ul>{1}</ul>";

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("SpecFlow Step Exporter");
                Console.WriteLine("Generates a simple HTML documentation with the supported steps (regular expressions) found in the assembly");
                Console.WriteLine("Usage: SupportedStepExporter [filepath to steps definition] [filename to export HTML to]");
                return;
            }

            GenerateDocumentation(args[0], args[1]);

            Console.WriteLine("Generated supported steps to {0}", args[1]);

        }

        private static void GenerateDocumentation(string stepDefinitionPath, string exportFilePath)
        {
            var a = Assembly.LoadFrom(stepDefinitionPath);

            var assemblyHeader = GetAssemlbyHeader(a);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(TITLE_FORMAT, assemblyHeader));
            sb.AppendLine(string.Format(HEADER_FORMAT, assemblyHeader));

            var givens = GetRegExpsForAttributeInTypes<GivenAttribute>(a);
            sb.AppendLine(string.Format(STEP_FORMAT, "Given", givens));

            var whens = GetRegExpsForAttributeInTypes<WhenAttribute>(a);
            sb.AppendLine(string.Format(STEP_FORMAT, "When", whens));

            var thens = GetRegExpsForAttributeInTypes<ThenAttribute>(a);
            sb.AppendLine(string.Format(STEP_FORMAT, "Then", thens));

            sb.AppendLine(FOOTER);

            ExportToHtmlFile(exportFilePath, sb);
        }

        private static void ExportToHtmlFile(string exportFilePath, StringBuilder htmlToWrite)
        {
            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            File.AppendAllText(exportFilePath, htmlToWrite.ToString());
        }

        private static string GetAssemlbyHeader(Assembly a)
        {
            return WebUtility.HtmlEncode("Assembly: " + a.GetName().Name + ", version: " + a.GetName().Version.ToString());
        }

        private static string GetRegExpsForAttributeInTypes<TAttribute>(Assembly assemblyToSearch) where TAttribute : ScenarioStepAttribute
        {
            var regexpes = from t in assemblyToSearch.GetTypes()
                           from m in t.GetMethods()
                           from a in m.GetCustomAttributes(typeof(TAttribute), true)
                           select GetHtmlForRegExp(a as ScenarioStepAttribute);

            return string.Join(Environment.NewLine, regexpes);
        }

        private static string GetHtmlForRegExp(ScenarioStepAttribute a)
        {
            var encodedRegExp = WebUtility.HtmlEncode(a.Regex);
            return string.Format(REGEXP_FORMAT, encodedRegExp);
        }

    }
}


