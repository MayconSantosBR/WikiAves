using Microsoft.AspNetCore.Mvc;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Family;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FamilyScrapperController : Controller
    {
        private readonly IFamilyScrapperService familyService;

        public FamilyScrapperController(IFamilyScrapperService familyService)
        {
            this.familyService = familyService;
        }

        [HttpGet("GetFamilies")]
        public async Task<ActionResult<List<Families>>> GetFamilies()
        {
            try
            {
                return Ok(await familyService.GetFamiliesAsync());
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
