using Newtonsoft.Json;

namespace WikiAvesScrapper.Models.Responses
{
    public class GetRegistrosResponse
    {
        [JsonProperty("id")]
        public int SoundId { get; set; }

        [JsonProperty("tipo")]
        public string SoundType { get; set; }

        [JsonProperty("id_usuario")]
        public string UserId { get; set; }

        [JsonProperty("sp")]
        public SpecieResponse Specie { get; set; }

        [JsonProperty("autor")]
        public string Author { get; set; }

        [JsonProperty("perfil")]
        public string Account { get; set; }

        [JsonProperty("data")]
        public string PostDate { get; set; }

        [JsonProperty("is_questionada")]
        public bool IsQuestion { get; set; }

        [JsonProperty("local")]
        public string Location { get; set; }

        [JsonProperty("idMunicipio")]
        public int CityId { get; set; }

        [JsonProperty("link")]
        public string S3BrowserLink { get; set; }

        [JsonProperty("dura")]
        public string Duration { get; set; }
    }

    public class SpecieResponse
    {
        [JsonProperty("id")]
        public string SpecieId { get; set; }

        [JsonProperty("nome")]
        public string Name { get; set; }

        [JsonProperty("nvt")]
        public string CommonName { get; set; }

        [JsonProperty("idwiki")]
        public string WikiName { get; set; }
    }
}
