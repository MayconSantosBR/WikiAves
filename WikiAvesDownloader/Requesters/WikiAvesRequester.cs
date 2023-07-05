using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAvesCore.Models.Classifications;
using WikiAvesDownloader.Requesters.Interfaces;

namespace WikiAvesDownloader.Requesters
{
    public class WikiAvesRequester : IWikiAvesRequester
    {
        private HttpClient client;

        public WikiAvesRequester(HttpClient client)
        {
            this.client = client;
            this.client.BaseAddress = new Uri("https://localhost:7173");
        }

        public async Task<List<Families>?> GetFamiliesAsync()
        {
            try
            {
                using var response = await client.GetAsync($"/index/scrapper/families");

                var families = JsonConvert.DeserializeObject<List<Families>?>(await response.Content.ReadAsStringAsync());

                return families;
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                throw;
            }
        }

        public async Task<List<Sounds>?> GetSpecieSoundsAsync(long specieId)
        {
            try
            {
                using var response = await client.GetAsync($"/sound/scrapper/specie/{specieId}");

                var sounds = JsonConvert.DeserializeObject<List<Sounds>?>(await response.Content.ReadAsStringAsync());

                return sounds;
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                return null;
            }
        }
    }
}
