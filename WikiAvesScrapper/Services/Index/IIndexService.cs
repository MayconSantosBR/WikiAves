using AngleSharp.Dom;
using FluentResults;
using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public interface IIndexService
    {
        Task<Result<List<Families>>> GetFamiliesAsync();
        Task<Result<List<Species>>> GetSpeciesAsync();
    }
}