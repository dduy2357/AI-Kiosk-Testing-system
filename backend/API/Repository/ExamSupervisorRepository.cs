using API.Models;
using API.Repository.Interface;

public class ExamSupervisorRepository : IExamSupervisorRepository
{
    private readonly Sep490Context _context;

    public ExamSupervisorRepository(Sep490Context context)
    {
        _context = context;
    }

    public async Task AddAsync(ExamSupervisor examSupervisor)
    {
        await _context.ExamSupervisors.AddAsync(examSupervisor);
    }
}
