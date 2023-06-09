using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace WikiAvesScrapper.Services.Base
{
    public class ScrapperBase
    {
        protected HttpClient client;

        public ScrapperBase(HttpClient client)
        {
            this.client = client;
            this.client.BaseAddress = new Uri(EnvironmentConfig.Hosts.WikiAves);
            this.client.Timeout = TimeSpan.FromMinutes(5);
        }
    }
}
