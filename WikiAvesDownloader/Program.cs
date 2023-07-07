using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WikiAves.Downloader;
using WikiAves.Downloader.Models;
using WikiAvesDownloader.Requesters;
using WikiAvesDownloader.Requesters.Interfaces;

var services = new ServiceCollection();
services.AddSingleton<IWikiAvesRequester, WikiAvesRequester>();
services.AddHttpClient<IWikiAvesRequester, WikiAvesRequester>();
services.AddAutoMapper(typeof(Mapper));
services.AddSingleton<IMongoClient>(c =>
{
    return new MongoClient("mongodb://localhost:27017");
});

Console.WriteLine("App initialized!");

using (App application = new(services))
{
    await application.Start();
};

Console.WriteLine("Done!");

