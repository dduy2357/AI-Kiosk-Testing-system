using API.Cached;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class CampusService : ICampusService
    {
        private readonly IDataCached _dataCached;
        public CampusService(IDataCached dataCached)
        {
            _dataCached = dataCached;
        }
        public async Task<(string, List<CampusVM>?)> GetAllCampusesAsync()
        {
            var (msg, campus) = await _dataCached.GetCampuses(); 
            if (msg.Length > 0)  return (msg, null);

            return ("", campus);
        }
    }
}
