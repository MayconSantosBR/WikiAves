using WikiAvesCore.Models.Archives;
using WikiAvesCore.Models.Specifications;

namespace WikiAvesCore.Models.Classifications
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
