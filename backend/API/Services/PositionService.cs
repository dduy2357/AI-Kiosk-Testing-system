using API.Cached;
using API.Commons;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class PositionService : IPositionService
    {
        private readonly IDataCached _dataCached;
        public PositionService(IDataCached dataCached)
        {
            _dataCached = dataCached ?? throw new ArgumentNullException(nameof(dataCached));
        }
        public async Task<(string, List<PositionVM>?)> GetPositionByDepartment(string departmentId)
        {
            var (msg, cached) = await _dataCached.GetPositions();
            if (msg.Length > 0) return (msg, null);

            var positions = cached?.Where(x => x.DepartmentId == departmentId).ToList();
            if (positions.IsObjectEmpty()) return ("No positions found.", null);
            return ("", positions);
        }
    }
}
