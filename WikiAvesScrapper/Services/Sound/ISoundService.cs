using FluentResults;
using WikiAvesScrapper.Models.Classifications;

namespace WikiAvesScrapper.Services.Sound
{
    public interface ISoundService
    {
        Task<Result<List<Sounds>>> GetSoundsByIdAsync(long specieId);
    }
}