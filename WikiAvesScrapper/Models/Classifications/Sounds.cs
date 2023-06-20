using WikiAvesScrapper.Models.Archives;
using WikiAvesScrapper.Models.Specifications;

namespace WikiAvesScrapper.Models.Classifications
{
    public class Sounds
    {
        public string SoundType { get; set; }
        public string SoundEmitter { get; set; }
        public Animals AnimalSpecifications { get; set; }
        public Files FileSpecifications { get; set; }
        public Locations Locations { get; set; }
    }
}
