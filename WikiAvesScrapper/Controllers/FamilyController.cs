using Microsoft.AspNetCore.Mvc;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Family;

namespace WikiAvesScrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FamilyController : Controller
    {
        private readonly IFamilyService familyService;

        public FamilyController(IFamilyService familyService)
        {
            this.familyService = familyService;
        }

        [HttpGet("GetFamilies")]
        public async Task<ActionResult<List<Families>>> GetFamilies()
        {
            try
            {
                var families = await familyService.GetFamiliesAsync();

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
    }
}
