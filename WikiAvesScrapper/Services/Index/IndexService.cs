﻿using AngleSharp.Dom;
using FluentResults;
using System.Text.RegularExpressions;
using WikiAvesCore.Models.Classifications;
using WikiAvesScrapper.Services.Base;
using WikiAvesScrapper.Util;
using WikiAvesCore.Util;

namespace WikiAvesScrapper.Services.Family
{
    public class IndexService : ScrapperBase, IIndexService
    {
        private readonly ILogger<IndexService> logger;
        public IndexService(HttpClient client, ILogger<IndexService> logger) : base(client)
        {
            this.logger = logger;
        }

        public async Task<Result<List<Families>>> GetFamiliesAsync(bool checkIntegrity = false)
        {
            try
            {
                List<Families> families = new();

                var indexElements = await GetIndexAsync();

                if (indexElements.IsFailed)
                    return Result.Fail(indexElements.Errors);

                foreach (var familyElement in indexElements.Value)
                {
                    var content = familyElement.InnerHtml;
                    content = content.UseRegex(@"\((.*?)\)").Replace("'", String.Empty);

                    var infoArray = familyElement.InnerHtml.Split(",");

                    if (infoArray == null)
                        continue;

                    if (infoArray[1].Split(" ").Count() == 1)
                    {
                        Families family = new();
                        family.Name = infoArray[1];
                        family.Uri = $"{EnvironmentConfig.Hosts.WikiAves}/wiki/{family.Name.ToLower()}";
                        families.Add(family);
                    }
                }

                if (checkIntegrity)
                {
                    await Parallel.ForEachAsync(families, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (family, token) =>
                    {
                        {
                            try
                            {
                                using var response = await client.GetAsync($"/wiki/{family.Name}");
                                var decodedHtml = await response.Content.ReadAsStringAsync();

                                if (response.IsSuccessStatusCode && !decodedHtml.ToLower().Contains("esse_topico_ainda_nao_existe"))
                                    family.IsActive = true;

                                family.LastCheck = DateTime.UtcNow.AddHours(-3);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    });
                }

                return Result.Ok(families);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }

        public async Task<Result<List<Species>>> GetSpeciesAsync(bool checkIntegrity = false)
        {
            try
            {
                List<Species> species = new();

                var indexElements = await GetIndexAsync();

                string lastFamily = "";
                foreach (var specieElement in indexElements.Value)
                {
                    var content = specieElement.InnerHtml;
                    content = content.UseRegex(@"\((.*)\)")
                        .Replace(", ", ",")
                        .Replace(",'", ",")
                        .Replace("',", ",");

                    var infoArray = content.Split(",");

                    if (infoArray == null)
                        continue;

                    if (!string.IsNullOrEmpty(infoArray[1]))
                        lastFamily = infoArray[1];

                    Species specie = new();
                    specie.SpecieId = Convert.ToInt64(infoArray[0].OnlyNumbers());
                    specie.Family = lastFamily;
                    specie.SpecieName = infoArray[2];
                    specie.CommonName = infoArray[3].Replace("\\", String.Empty);
                    specie.ImageQuantity = Convert.ToInt64(infoArray[5].OnlyNumbers());
                    specie.SoundQuantity = Convert.ToInt64(infoArray[6].OnlyNumbers());
                    specie.Uri = $"{EnvironmentConfig.Hosts.WikiAves}/wiki/{specie.CommonName.ToLower().Replace("'", "_")}";

                    species.Add(specie);
                }

                if (checkIntegrity)
                {
                    await Parallel.ForEachAsync(species, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (specie, token) =>
                    {
                        {
                            try
                            {
                                var response = await client.GetAsync(specie.Uri);
                                var decodedHtml = await response.Content.ReadAsStringAsync();

                                if (response.IsSuccessStatusCode && !decodedHtml.ToLower().Contains("esse_topico_ainda_nao_existe"))
                                    specie.IsActive = true;

                                specie.LastCheck = DateTime.UtcNow.AddHours(-3);
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }
                    });
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

                var html = await content.LoadHtmlAsync();
                var indexElements = html.GetElementsByTagName("script").Where(c => c.InnerHtml.Contains("lsp('"));

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
