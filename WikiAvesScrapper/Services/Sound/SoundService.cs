using AngleSharp;
using FluentResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using WikiAvesCore.Models.Classifications;
using WikiAvesScrapper.Models.Responses;
using WikiAvesScrapper.Services.Base;
using WikiAvesScrapper.Util;
using WikiAvesCore.Util;

namespace WikiAvesScrapper.Services.Sound
{
    public class SoundService : ScrapperBase, ISoundService
    {
        private readonly ILogger<SoundService> logger;

        public SoundService(HttpClient client, ILogger<SoundService> logger) : base(client)
        {
            this.logger = logger;
        }

        public async Task<Result<List<Sounds>>> GetSoundsByIdAsync(long specieId)
        {
            try
            {
                List<Sounds> sounds = new();

                using var response = await client.GetAsync($"{EnvironmentConfig.Hosts.WikiAves}/getRegistrosJSON.php?tm=s&t=s&s={specieId}&o=mp&o=mp&p=1");

                var registersObject = await response.Content.ReadAsStringAsync();
                var stdJsonContent = JToken.Parse(registersObject); //text["registros"]["itens"][0]["1"]

                var itens = stdJsonContent["registros"]["itens"].Count();

                for (int i = 1; i <= Convert.ToInt16(itens); i++)
                {
                    Sounds sound = new()
                    {
                        FileSpecifications = new(),
                        Locations = new()
                    };

                    var desserializedSound = JsonConvert.DeserializeObject<GetRegistrosResponse>(stdJsonContent["registros"]["itens"][Convert.ToString(i)].ToString());

                    sound.SoundType = desserializedSound.SoundType;
                    sound.FileSpecifications.Duration = Convert.ToInt32(desserializedSound.Duration.OnlyNumbers());
                    sound.FileSpecifications.LinkForImage = desserializedSound.S3BrowserLink.Replace("#", string.Empty);
                    sound.FileSpecifications.LinkForSound = desserializedSound.S3BrowserLink.Replace("#", string.Empty).Replace(".jpg", ".mp3");

                    var location = desserializedSound.Location.Split('/');
                    sound.Locations.City = location[0];
                    sound.Locations.State = location[1];

                    sounds.Add(sound);
                }

                return Result.Ok(sounds);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
    }
}
