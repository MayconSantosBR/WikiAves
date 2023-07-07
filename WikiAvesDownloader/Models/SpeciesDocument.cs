using MongoDB.Bson.Serialization.Attributes;
using System;
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

        [BsonElement("is_active")]
        public bool? IsActive { get; set; }

        [BsonElement("check_date")]
        public DateTime? LastCheck { get; set; }

        [BsonElement("sounds")]
        public List<Sounds> Sounds { get; set; } = new();
    }
}
