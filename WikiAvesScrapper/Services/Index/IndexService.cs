using AngleSharp;
using AngleSharp.Dom;
using FluentResults;
using WikiAvesScrapper.Data.Loaders;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Base;

namespace WikiAvesScrapper.Services.Family
{
    public class IndexService : ScrapperBase, IIndexService
    {
        private readonly ILogger<IndexService> logger;
        public IndexService(HttpClient client, ILogger<IndexService> logger) : base(client)
        {
            this.logger = logger;
        }

        public async Task<Result<List<Families>>> GetFamiliesAsync()
        {
            try
            {
                List<Families> families = new();
                List<string> specials = new() { "lsp", " " };

                var indexElements = await GetIndexAsync();

                if (indexElements.IsFailed)
                    return Result.Fail(indexElements.Errors);

                foreach (var familyElement in indexElements.Value)
                {
                    CleanContent(familyElement, true, specials);

                    var infoArray = familyElement.InnerHtml.Split(",");

                    if (infoArray == null)
                        continue;

                    if (infoArray[1].Split(" ").Count() == 1)
                    {
                        Families family = new() { Name = infoArray[1] };
                        families.Add(family);
                    }
                }

                foreach (var family in families)
                {
                    using var response = await client.GetAsync($"/wiki/{family.Name}");

                    var decodedHtml = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && !decodedHtml.ToLower().Contains("esse_topico_ainda_nao_existe"))
                    {
                        family.Uri = $"{EnvironmentConfig.Hosts.WikiAves}/wiki/{family.Name.ToLower()}";
                        family.IsActive = true;
                    }

                    family.LastCheck = DateTime.UtcNow.AddHours(-3);
                }

                return Result.Ok(families);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }

        public async Task<Result<List<Species>>> GetSpeciesAsync()
        {
            try
            {
                List<Species> species = new();
                List<string> specials = new() { " lsp" };

                var indexElements = await GetIndexAsync();

                string lastFamily = "";
                foreach (var specieElement in indexElements.Value)
                {
                    CleanContent(specieElement, true, specials);
                    specieElement.InnerHtml = specieElement.InnerHtml.Replace(", ", ",");

                    var infoArray = specieElement.InnerHtml.Split(",");

                    if (infoArray == null)
                        continue;

                    if (!string.IsNullOrEmpty(infoArray[1]))
                        lastFamily = infoArray[1];

                    Species specie = new();
                    specie.SpecieId = Convert.ToInt64(infoArray[0]);
                    specie.Family = lastFamily;
                    specie.SpecieName = infoArray[2];
                    specie.CommonName = infoArray[3];
                    specie.ImageQuantity = Convert.ToInt64(infoArray[5]);
                    specie.SoundQuantity = Convert.ToInt64(infoArray[6]);

                    species.Add(specie);
                }

                return Result.Ok(species);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }

        private async Task<Result<IEnumerable<IElement>>> GetIndexAsync()
        {
            try
            {
                using var response = await client.GetAsync("/especies.php?t=t");
                var content = await response.Content.ReadAsStringAsync();

                var loader = new HtmlLoader(content);
                var indexElements = loader.document.GetElementsByTagName("script").Where(c => c.InnerHtml.Contains("lsp('"));

                if (indexElements == null)
                    return Result.Fail("Not found any elements at this page.");
                else
                    return Result.Ok(indexElements);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
    }
}
