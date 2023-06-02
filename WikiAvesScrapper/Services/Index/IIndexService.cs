using AngleSharp.Dom;
using FluentResults;
using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public interface IIndexService
    {
        Task<Result<IEnumerable<IElement>>> GetIndexAsync();
    }
}