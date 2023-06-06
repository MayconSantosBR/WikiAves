using AngleSharp.Dom;
namespace WikiAvesScrapper.Services.Base
{
    public class ScrapperBase
    {
        protected HttpClient client;
        private static readonly List<string> chars = new() { "<", ">", "(", " )", ")", " '", "'", ";" };

        public ScrapperBase(HttpClient client)
        {
            this.client = client;
            this.client.BaseAddress = new Uri(EnvironmentConfig.Hosts.WikiAves);
        }

        protected void CleanContent(IElement? content, bool clearChars = false, List<string> clearSpecialsList = null)
        {
            if (clearChars)
                foreach (var c in chars)
                    content.InnerHtml = content.InnerHtml.Replace(c, String.Empty);

            if (clearSpecialsList != null)
                foreach (var c in clearSpecialsList)
                    content.InnerHtml = content.InnerHtml.Replace(c, String.Empty);
        }
    }
}
