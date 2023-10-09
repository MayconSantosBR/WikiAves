using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WikiAvesCore.Models.Classifications;
using WikiAvesScrapper.Services.Family;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : Controller
    {
        private readonly IMemoryCache memoryCache;
        private readonly IIndexService indexService;

        public IndexController(IIndexService familyService, IMemoryCache memoryCache)
        {
            this.indexService = familyService;
            this.memoryCache = memoryCache;
        }

        [HttpGet("scrapper/families")]
        public async Task<ActionResult<List<Families>>> GetFamilies
            (
                [FromQuery] bool checkIntegrity = false
            )
        {
            try
            {
                var families = await memoryCache.GetOrCreateAsync("FamiliesCache", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    return await indexService.GetFamiliesAsync();
                });

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

        [HttpGet("scrapper/species")]
        public async Task<ActionResult<List<Species>>> GetSpecies
            (
                [FromQuery] bool checkIntegrity = false
            )
        {
            try
            {
                var species = await memoryCache.GetOrCreateAsync("SpeciesCache", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    return await indexService.GetSpeciesAsync(checkIntegrity);
                });

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
