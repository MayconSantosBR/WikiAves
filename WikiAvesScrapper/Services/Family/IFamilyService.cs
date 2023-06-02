using FluentResults;
using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public interface IFamilyService
    {
        Task<Result<List<Families>>> GetFamiliesAsync();
    }
}