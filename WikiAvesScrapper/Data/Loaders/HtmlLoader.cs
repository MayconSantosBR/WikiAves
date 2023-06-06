using AngleSharp;
using AngleSharp.Dom;

namespace WikiAvesScrapper.Data.Loaders
{
    public class HtmlLoader
    {
        public IDocument document;
        public HtmlLoader(string content)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            document = context.OpenAsync(c => c.Content(content)).Result;
        }
    }
}
