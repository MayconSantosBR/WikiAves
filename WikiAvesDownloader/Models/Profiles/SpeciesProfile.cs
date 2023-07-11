using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiAvesCore.Models.Classifications;

namespace WikiAves.Downloader.Models.Profiles
{
    public class SpeciesProfile : Profile
    {
        public SpeciesProfile()
        {
            CreateMap<Species, SpeciesDocument>();
            CreateMap<SpeciesDocument, Species>()
                .ForSourceMember(src => src.Sounds, c => c.DoNotValidate())
                .ForSourceMember(src => src.Id, c => c.DoNotValidate());
        }
    }
}
