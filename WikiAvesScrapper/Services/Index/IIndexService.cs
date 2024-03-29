﻿using FluentResults;
using WikiAvesCore.Models.Classifications;

namespace WikiAvesScrapper.Services.Family
{
    public interface IIndexService
    {
        Task<Result<List<Families>>> GetFamiliesAsync(bool checkIntegrity = false);
        Task<Result<List<Species>>> GetSpeciesAsync(bool checkIntegrity = false);
    }
}