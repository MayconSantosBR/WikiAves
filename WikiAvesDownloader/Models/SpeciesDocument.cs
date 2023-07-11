using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Core.Models.Classifications.Interfaces;
using WikiAvesCore.Models.Classifications;

namespace WikiAves.Downloader.Models
{
    public class SpeciesDocument : ISpecies
    {
        [BsonElement("_id")]
        [JsonProperty("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("id")]
        public long SpecieId { get; set; }

        [BsonElement("family")]
        public string Family { get; set; }

        [BsonElement("name")]
        public string SpecieName { get; set; }

        [BsonElement("common_name")]
        public string CommonName { get; set; }

        [BsonElement("uri")]
        public string Uri { get; set; }


        [BsonElement("check_date")]
        public DateTime? LastCheck { get; set; }

        [BsonElement("sounds")]
        public List<Sounds> Sounds { get; set; } = new();

        public override bool Equals(object? other)
        {
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.GetValue(other) == null && property.GetValue(this) == null)
                    continue;
                else if (property.GetValue(other).ToString() != property.GetValue(this).ToString())
                    return false;
            }

            return true;
        }
    }
}
