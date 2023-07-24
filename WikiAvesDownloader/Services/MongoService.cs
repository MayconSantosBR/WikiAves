using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Core.Util;
using WikiAves.Downloader.Models;
using WikiAves.Downloader.Services.Interfaces;
using WikiAves.Downloader.Util;
using WikiAvesCore.Models.Classifications;
using WikiAvesDownloader.Requesters;
using WikiAvesDownloader.Requesters.Interfaces;

namespace WikiAves.Downloader.Services
{
    public class MongoService : IMongoService
    {
        private readonly IMongoClient client;
        private readonly IWikiAvesRequester wikiAvesRequester;
        private readonly IMapper mapper;
        private HttpClient httpClient;


        public MongoService(IMongoClient client, IWikiAvesRequester wikiAvesRequester, IMapper mapper, HttpClient httpClient)
        {
            this.client = client;
            this.wikiAvesRequester = wikiAvesRequester;
            this.mapper = mapper;
            this.httpClient = httpClient;
        }

        public async Task SaveSpeciesAsync()
        {
            try
            {
                var species = await wikiAvesRequester.GetIndexesAsync<Species>();

                if (species == null)
                    throw new Exception("Nothing was found for species!");

                var database = client.GetDatabase("wikiaves");
                var collection = database.GetCollection<SpeciesDocument>("species", new MongoCollectionSettings() { AssignIdOnInsert = false });

                List<WriteModel<SpeciesDocument>> createSpecies = new();

                await Parallel.ForEachAsync(species, new ParallelOptions() { MaxDegreeOfParallelism = 15 }, async (specie, token) =>
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

        public async Task DownloadSpeciesAsync()
        {
            try
            {
                var database = client.GetDatabase("wikiaves");
                var collection = database.GetCollection<SpeciesDocument>("species", new MongoCollectionSettings() { AssignIdOnInsert = false });

                var speciesWithSound = await collection.FindAsync(c => c.Sounds.Count() != 0);

                do
                {
                    if (await speciesWithSound.MoveNextAsync())
                    {
                        var documents = speciesWithSound.Current;

                        await Parallel.ForEachAsync(documents, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, async (document, token) =>
                        {
                            foreach (var sound in document.Sounds)
                            {
                                try
                                {
                                    var identifier = sound.FileSpecifications.LinkForSound
                                                            .Split("_")
                                                            .ToList()
                                                            .ElementAt(1)
                                                            .Replace(".mp3", string.Empty);

                                    var pathForSound = @$"C:\Estudo\WikiAvesSounds\Sounds\{document.CommonName}-{identifier}.mp3";
                                    var pathForWav = $@"C:\Estudo\WikiAvesSounds\Sounds\Wav\{document.CommonName}-{identifier}.wav";

                                    if (File.Exists(pathForSound))
                                        continue;

                                    await httpClient.DownloadFileTaskAsync(sound.FileSpecifications.LinkForSound, pathForSound);

                                    AudioExtensions.ConvertMp3ToWav(pathForSound, pathForWav);

                                    await Console.Out.WriteLineAsync($"[{document.SpecieId}][{document.CommonName}] Sound {document.Sounds.IndexOf(sound) + 1} downloaded!");
                                }
                                catch (Exception e)
                                {
                                    await Console.Out.WriteLineAsync($"[{document.SpecieId}][{document.CommonName}] An error was ocurred during the download. Message: {e.Message}");
                                    return;
                                }
                            }
                        });
                    }
                    else
                    {
                        break;
                    }
                } while (true);
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }
    }
}
