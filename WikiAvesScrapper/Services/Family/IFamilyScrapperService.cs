using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public interface IFamilyScrapperService
    {
        Task<List<Families>> GetFamiliesAsync();
    }
}