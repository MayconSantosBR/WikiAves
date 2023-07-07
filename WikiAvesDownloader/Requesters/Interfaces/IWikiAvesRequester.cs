using WikiAvesCore.Models.Classifications;

namespace WikiAvesDownloader.Requesters.Interfaces
{
    public interface IWikiAvesRequester
    {
        Task<List<T>?> GetIndexesAsync<T>();
        Task<List<Sounds>?> GetSpecieSoundsAsync(long specieId);
    }
}