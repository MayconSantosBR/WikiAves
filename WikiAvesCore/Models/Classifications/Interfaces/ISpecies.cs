namespace WikiAves.Core.Models.Classifications.Interfaces
{
    public interface ISpecies
    {
        public long SpecieId { get; set; }
        public string Family { get; set; }
        public string SpecieName { get; set; }
        public string CommonName { get; set; }
        public string Uri { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? LastCheck { get; set; }
    }
}