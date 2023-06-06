using Microsoft.AspNetCore.Mvc;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Family;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : Controller
    {
        private readonly IIndexService indexService;

        public IndexController(IIndexService familyService)
        {
            this.indexService = familyService;
        }

        [HttpGet("GetFamilies")]
        public async Task<ActionResult<List<Families>>> GetFamilies()
        {
            try
            {
                var families = await indexService.GetFamiliesAsync();

                if (families.IsSuccess)
                    return Ok(families.Value);
                else
                    return BadRequest(families.Errors);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetSpecies")]
        public async Task<ActionResult<List<Species>>> GetSpecies()
        {
            try
            {
                var species = await indexService.GetSpeciesAsync();

                if (species.IsSuccess)
                    return Ok(species.Value);
                else
                    return BadRequest(species.Errors);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
