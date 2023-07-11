using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Downloader.Services.Interfaces;

namespace WikiAves.Downloader.Services
{
    public class MongoService : IMongoService
    {
        private readonly IMongoClient client;

        public MongoService(IMongoClient client)
        {
            this.client = client;
        }
    }
}
