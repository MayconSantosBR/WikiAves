using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Sound;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SoundController : Controller
    {
        private readonly ISoundService soundService;

        public SoundController(ISoundService soundService)
        {
            this.soundService = soundService;
        }

        [HttpGet("GetSoundsBySpecieId")]
        public async Task<ActionResult<List<string>>> GetSoundsBySpecieId(long specieId)
        {
            try
            {
                var sounds = await soundService.GetSoundsByIdAsync(specieId);

                if (sounds.IsSuccess)
                    return Ok(sounds.Value);
                else
                    return BadRequest(sounds.Errors);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
