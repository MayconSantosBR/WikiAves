using Microsoft.AspNetCore.Mvc;

namespace WikiAves.AudioRecognizer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SoundController : ControllerBase
    {
        private readonly ILogger<SoundController> _logger;

        public SoundController(ILogger<SoundController> logger)
        {
            _logger = logger;
        }

        [HttpPost("recognizer/sound/recognize")]
        public async Task<ActionResult> Recognize()
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}