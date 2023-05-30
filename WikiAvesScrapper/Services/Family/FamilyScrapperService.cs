using HtmlAgilityPack;
using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public class FamilyScrapperService : IFamilyScrapperService
    {
		public HttpClient client { get; set; }

        public FamilyScrapperService(HttpClient client)
        {
            this.client = client;
            this.client.BaseAddress = new Uri("https://www.wikiaves.com.br");
        }

        public async Task<List<Families>> GetFamiliesAsync()
        {
            List<Families> families = new();
            HtmlDocument document = new();

            try
			{
                using var response = await client.GetAsync("/especies.php?t=t");
                document.LoadHtml(await response.Content.ReadAsStringAsync());
                //document.DocumentNode.Element("script")
                return families;
			}
			catch
			{
				throw;
			}
        }
    }
}
