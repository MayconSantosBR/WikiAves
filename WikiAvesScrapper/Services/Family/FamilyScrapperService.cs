using AngleSharp;
using AngleSharp.Dom;
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

            try
			{
                var response = await client.GetAsync("/especies.php?t=t");
                var content = await response.Content.ReadAsStringAsync();

                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var doc = await context.OpenAsync(req => req.Content(content));

                var speciesElement = doc.GetElementsByTagName("script").Where(c => c.InnerHtml.Contains("lsp('"));
                foreach (var specie in speciesElement)
                {
                    List<string> chars = new() { "lsp", "(", ")", "'", ";", " " };

                    CleanContent(specie, chars);

                    var infoArray = specie.InnerHtml.Split(",");

                    if (infoArray == null)
                        continue;

                    if (infoArray[1].Split(" ").Count() == 1 && infoArray[1].EndsWith("ae"))
                    {
                        Families family = new() { Name = infoArray[1] };
                        families.Add(family);
                    }
                }

                foreach (var family in families)
                {
                    response = await client.GetAsync($"/wiki/{family.Name}");
                    content = await response.Content.ReadAsStringAsync();
                    doc = await context.OpenAsync(c => c.Content(content));

                    var paragraphs = doc.GetElementsByTagName("p");
                }

                return families;
			}
			catch
			{
				throw;
			}
        }

        private void CleanContent(IElement? specie, List<string> chars)
        {
            foreach (var c in chars)
                specie.InnerHtml = specie.InnerHtml.Replace(c, "");
        }
    }
}
