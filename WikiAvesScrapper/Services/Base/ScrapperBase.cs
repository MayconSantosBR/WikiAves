using AngleSharp.Dom;
namespace WikiAvesScrapper.Services.Base
{
    public class ScrapperBase
    {
        protected HttpClient client;
        private static readonly List<string> chars = new() { "<", ">", "(", ")", "'", ";" };
        private static readonly List<string> specials = new() { "lsp", " " };

        public ScrapperBase(HttpClient client)
        {
            this.client = client;
            this.client.BaseAddress = new Uri(EnvironmentConfig.Hosts.WikiAves);
        }

        protected void CleanContent(
            IElement? content, 
            bool clearChars = false,
            bool clearSpecials = false)
        {
            if (clearChars)
                foreach (var c in chars)
                    content.InnerHtml = content.InnerHtml.Replace(c, "");

            if (clearSpecials)
                foreach (var c in specials)
                    content.InnerHtml = content.InnerHtml.Replace(c, "");
        }
    }
}
