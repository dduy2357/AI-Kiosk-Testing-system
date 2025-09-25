using API.Cached;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class MajorService : IMajorService
    {
        private readonly IDataCached _dataCached;
        public MajorService(IDataCached dataCached)
        {
            _dataCached = dataCached ?? throw new ArgumentNullException(nameof(dataCached));
        }
        public async Task<(string, List<MajorVM>?)> GetAllMajorsAsync()
        {
            var (msg, majors) = await _dataCached.GetMajors();
            if (msg.Length > 0) return (msg, null);

            return ("", majors);
        }
    }
}
