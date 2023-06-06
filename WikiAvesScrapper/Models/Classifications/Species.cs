namespace WikiAvesScrapper.Models.Classifications
{
    public class Species
    {
        public long SpecieId { get; set; }
        public string Family { get; set; }
        public string SpecieName { get; set; }
        public string CommonName { get; set; }
        public long SoundQuantity { get; set; }
        public long ImageQuantity { get; set; }
    }
}
