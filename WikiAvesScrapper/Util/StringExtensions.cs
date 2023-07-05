using AngleSharp;
using AngleSharp.Dom;
using IConfiguration = AngleSharp.IConfiguration;

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

        public async static Task<IDocument> LoadHtmlAsync(this string content, IConfiguration config)
        {
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(c => c.Content(content));
        }
    }
}
