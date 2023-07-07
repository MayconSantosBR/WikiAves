using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Core.Models.Classifications.Interfaces;
using WikiAves.Downloader.Models;
using WikiAvesCore.Models.Classifications;
using WikiAvesDownloader.Requesters.Interfaces;
using Mapper = WikiAves.Downloader.Models.Mapper;

namespace WikiAves.Downloader
{
    public class App : IDisposable
    {
        private readonly IWikiAvesRequester wikiAvesRequester;
        private readonly IMongoClient mongoClient;
        private readonly IMapper mapper;

        public App(IServiceCollection services)
        {
            var builder = services.BuildServiceProvider();
            this.wikiAvesRequester = builder.GetService<IWikiAvesRequester>() ?? throw new ArgumentNullException();
            this.mongoClient = builder.GetService<IMongoClient>() ?? throw new ArgumentNullException();
            this.mapper = builder.GetService<IMapper>() ?? throw new ArgumentNullException();
        }

        public async Task Start()
        {
            try
            {
                var species = await wikiAvesRequester.GetIndexesAsync<Species>();

                if (species == null)
                    throw new Exception("Nothing was found for species!");

                List<WriteModel<SpeciesDocument>> speciesDocument = new();

                await Parallel.ForEachAsync(species, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async (specie, token) =>
                {
                    var document = mapper.Map<SpeciesDocument>(specie);
                    document.Sounds = await wikiAvesRequester.GetSpecieSoundsAsync(specie.SpecieId);
                    speciesDocument.Add(new InsertOneModel<SpeciesDocument>(document));
                });

                var database = mongoClient.GetDatabase("wikiaves");
                var collection = database.GetCollection<SpeciesDocument>("species", new MongoCollectionSettings() { AssignIdOnInsert = false });
                await collection.BulkWriteAsync(speciesDocument);
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
