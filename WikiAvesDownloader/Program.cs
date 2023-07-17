using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WikiAves.Downloader;
using WikiAves.Downloader.Models;
using WikiAves.Downloader.Services;
using WikiAves.Downloader.Services.Interfaces;
using WikiAvesDownloader.Requesters;
using WikiAvesDownloader.Requesters.Interfaces;

var services = new ServiceCollection();
services.AddSingleton<IMongoClient>(c => { return new MongoClient("mongodb://localhost:27017"); });
services.AddSingleton<IMongoService, MongoService>();
services.AddSingleton<IWikiAvesRequester, WikiAvesRequester>();
services.AddHttpClient<IWikiAvesRequester, WikiAvesRequester>();
services.AddHttpClient<IMongoClient, MongoClient>();
services.AddAutoMapper(typeof(Mapper));

Console.WriteLine("App initialized!");

using (App application = new(services))
{
    await application.Start();
};

Console.WriteLine("Done!");

