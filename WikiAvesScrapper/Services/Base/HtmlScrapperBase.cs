using AngleSharp;
using AngleSharp.Dom;

namespace WikiAvesScrapper.Services.Base
{
    public class HtmlScrapperBase
    {
        public IDocument document;
        public HtmlScrapperBase(string content)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            this.document = context.OpenAsync(c => c.Content(content)).Result;
        }
    }
}
