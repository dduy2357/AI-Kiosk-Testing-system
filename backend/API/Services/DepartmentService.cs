using API.Cached;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDataCached _dataCached;
        public DepartmentService(IDataCached dataCached)
        {
            _dataCached = dataCached ?? throw new ArgumentNullException(nameof(dataCached));
        }
        public async Task<(string, List<DepartmentVM>?)> GetAllDepartments()
        {
            var (msg, departments) = await _dataCached.GetDepartments();
            if (msg.Length > 0) return (msg, null);

            return ("", departments);
        }
    }
}
