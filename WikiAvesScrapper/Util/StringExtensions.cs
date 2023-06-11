using AngleSharp;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace WikiAvesScrapper.Util
{
    public static class StringExtensions
    {
        public async static Task<IDocument> LoadHtmlAsync(this string content)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(c => c.Content(content));
        }

        public static string UseRegex(this string text, string expression, int position = 1)
        {
            Regex regex = new Regex(expression, RegexOptions.None);
            return regex.Match(text).Groups[position].Value.ToString();
        }

        public static string OnlyNumbers(this string text)
        {
            return new String(text.Where(Char.IsDigit).ToArray());
        }
    }
}
