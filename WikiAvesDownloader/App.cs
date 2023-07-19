using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Core.Models.Classifications.Interfaces;
using WikiAves.Downloader.Models;
using WikiAves.Downloader.Services.Interfaces;
using WikiAvesCore.Models.Classifications;
using WikiAvesDownloader.Requesters.Interfaces;
using Mapper = WikiAves.Downloader.Models.Mapper;

namespace WikiAves.Downloader
{
    public class App : IDisposable
    {
        private readonly IMongoService mongoService;

        public App(IServiceCollection services)
        {
            var builder = services.BuildServiceProvider();
            this.mongoService = builder.GetService<IMongoService>() ?? throw new ArgumentNullException();
        }

        public async Task Start()
        {
            try
            {
                //await mongoService.SaveSpeciesAsync();
                await mongoService.DownloadSpeciesAsync();
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
