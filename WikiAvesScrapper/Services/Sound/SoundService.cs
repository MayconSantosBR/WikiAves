﻿using AngleSharp;
using FluentResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Models.Responses;
using WikiAvesScrapper.Services.Base;
using WikiAvesScrapper.Util;

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

                var itens = stdJsonContent["registros"]["total"];

                for (int i = 1; i < Convert.ToInt16(itens); i++)
                {
                    Sounds sound = new()
                    {
                        FileSpecifications = new(),
                        Others = new()
                        {
                            Locations = new(),
                            Author = new()
                        },
                    };

                    var desserializedSound = JsonConvert.DeserializeObject<GetRegistrosResponse>(stdJsonContent["registros"]["itens"][Convert.ToString(i)].ToString());

                    sound.SoundType = desserializedSound.SoundType;
                    sound.FileSpecifications.Duration = Convert.ToInt32(desserializedSound.Duration.OnlyNumbers());
                    sound.FileSpecifications.LinkForImage = desserializedSound.S3BrowserLink.Replace("#", string.Empty);
                    sound.FileSpecifications.LinkForSound = desserializedSound.S3BrowserLink.Replace("#", string.Empty).Replace(".jpg", ".mp3");

                    var location = desserializedSound.Location.Split('/');
                    sound.Others.Locations.City = location[0];
                    sound.Others.Locations.State = location[1];
                    sound.Others.Author.Name = desserializedSound.Author;
                    sound.Others.Author.Username = desserializedSound.Account;

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
