namespace WikiAves.Downloader.Services.Interfaces
{
    public interface IMongoService
    {
        Task SaveSpeciesAsync();
        Task DownloadSpeciesAsync();
    }
}