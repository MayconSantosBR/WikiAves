using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WikiAvesScrapper.Services.Sound;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SoundController : Controller
    {

        [HttpGet("scrapper/specie/{specieId}")]
        public async Task<ActionResult<List<string>>> GetSoundsBySpecieId(
            [FromRoute] long specieId,
            [FromServices] ISoundService soundService
            )
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
