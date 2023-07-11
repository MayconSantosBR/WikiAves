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

                var database = mongoClient.GetDatabase("wikiaves");
                var collection = database.GetCollection<SpeciesDocument>("species", new MongoCollectionSettings() { AssignIdOnInsert = false });

                List<WriteModel<SpeciesDocument>> createSpecies = new();

                await Parallel.ForEachAsync(species, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, async (specie, token) =>
                {
                    try
                    {
                        var document = mapper.Map<SpeciesDocument>(specie);
                        document.Sounds = await wikiAvesRequester.GetSpecieSoundsAsync(specie.SpecieId);

                        var mongoDocument = await collection.FindAsync(c => c.SpecieId == document.SpecieId);
                        var copy = await mongoDocument.FirstOrDefaultAsync();

                        if (copy == null)
                        {
                            await Console.Out.WriteLineAsync($"[{document.SpecieId}][{document.CommonName}] added to creation list!");
                            (document.Id, document.LastCheck) = (ObjectId.GenerateNewId(), DateTime.UtcNow.AddHours(-3));
                            createSpecies.Add(new InsertOneModel<SpeciesDocument>(document));
                            return;
                        }

                        (document.Id, document.LastCheck) = (copy.Id, copy.LastCheck);

                        if (!document.Equals(copy) || !copy.LastCheck.HasValue || copy.LastCheck.Value.Date < DateTime.UtcNow.Date)
                        {
                            await Console.Out.WriteLineAsync($"[{document.SpecieId}][{document.CommonName}] updated!");
                            document.LastCheck = DateTime.UtcNow.AddHours(-3);
                            await collection.ReplaceOneAsync(c => c.SpecieId == document.SpecieId, document);
                        }
                    }
                    catch (Exception e)
                    {
                        await Console.Out.WriteLineAsync(e.Message);
                        return;
                    }
                });

                if (createSpecies.Count > 0)
                    await collection.BulkWriteAsync(createSpecies);
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
