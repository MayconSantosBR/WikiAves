using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAves.Downloader.Models.Profiles;

namespace WikiAves.Downloader.Models
{
    public class Mapper
    {
        public Mapper()
        {
            MapperConfiguration configuration = new(cfg =>
            {
                cfg.AddProfile<SpeciesProfile>();
            });

            configuration.CreateMapper();
        }
    }
}
