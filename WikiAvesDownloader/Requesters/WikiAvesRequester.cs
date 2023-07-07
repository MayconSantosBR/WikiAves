using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Core.Models.Classifications.Interfaces;
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

        public async Task<List<T>?> GetIndexesAsync<T>()
        {
            try
            {
                var obj = typeof(T);

                using var response = await client.GetAsync($"/index/scrapper/{obj.Name.ToLower()}");

                var indexes = JsonConvert.DeserializeObject<List<T>?>(await response.Content.ReadAsStringAsync());

                return indexes;
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
