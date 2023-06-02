using AngleSharp;
using AngleSharp.Dom;
using FluentResults;
using WikiAvesScrapper.Models.Classifications;
using WikiAvesScrapper.Services.Base;

namespace WikiAvesScrapper.Services.Family
{
    public class IndexService : ScrapperBase, IIndexService
    {
        private readonly ILogger<IndexService> logger;
        public IndexService(HttpClient client, ILogger<IndexService> logger) : base(client)
        {
            this.logger = logger;
        }

        public async Task<Result<IEnumerable<IElement>>> GetIndexAsync()
        {
            try
			{
                using var response = await client.GetAsync("/especies.php?t=t");
                var content = await response.Content.ReadAsStringAsync();

                var loader = new HtmlScrapperBase(content);
                var indexElements = loader.document.GetElementsByTagName("script").Where(c => c.InnerHtml.Contains("lsp('"));

                if (indexElements == null)
                    return Result.Fail("Not found any elements at this page.");
                else
                    return Result.Ok(indexElements);
			}
			catch (Exception e)
			{
				return Result.Fail(e.Message);
			}
        }
    }
}
