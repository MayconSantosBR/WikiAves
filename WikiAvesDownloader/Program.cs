using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WikiAvesDownloader.Requesters;
using WikiAvesDownloader.Requesters.Interfaces;

var services = new ServiceCollection();
services.AddSingleton<IWikiAvesRequester, WikiAvesRequester>();
services.AddHttpClient<IWikiAvesRequester, WikiAvesRequester>();
var builder = services.BuildServiceProvider();

Console.WriteLine("App initialized!");

var requester = builder.GetService<IWikiAvesRequester>();
Console.WriteLine(JsonConvert.SerializeObject(await requester.GetSpecieSoundsAsync(10001)));

Console.WriteLine("Done!");

