using API.Cached;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly IDataCached _dataCached;
        public SpecializationService(IDataCached dataCached)
        {
           _dataCached = dataCached ?? throw new ArgumentNullException(nameof(dataCached));
        }
        public async Task<(string, List<SpecializationVM>?)> GetAllSpecializationsAsync()
        {
            var (msg, data) = await _dataCached.GetSpecializations();
            if (msg.Length > 0) return (msg, null);

            return ("", data);
        }
    }
}
