using AngleSharp.Io;
using FluentResults;
using System.Reflection.Metadata.Ecma335;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Base;

namespace WikiAvesScrapper.Services.Family
{
    public class FamilyService : ScrapperBase, IFamilyService
    {
        private readonly IIndexService indexService;
        public FamilyService(HttpClient client, IIndexService indexService) : base(client)
        {
            this.indexService = indexService;
        }
        public async Task<Result<List<Families>>> GetFamiliesAsync()
        {
            try
            {
                List<Families> families = new();

                var indexElements = await indexService.GetIndexAsync();

                if (indexElements.IsFailed)
                    return Result.Fail(indexElements.Errors);

                foreach (var specie in indexElements.Value)
                {
                    CleanContent(specie, true, true);

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
    }
}
