using WikiAvesCore.Models.Classifications;

namespace WikiAvesDownloader.Requesters.Interfaces
{
    public interface IWikiAvesRequester
    {
        Task<List<Families>?> GetFamiliesAsync();
        Task<List<Sounds>?> GetSpecieSoundsAsync(long specieId);
    }
}