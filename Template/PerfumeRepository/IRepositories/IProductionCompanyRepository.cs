using PerfumeRepository.Models;
using System.Collections.Generic;

namespace PerfumeRepository.IRepositories
{
    public interface IProductionCompanyRepository : IRepositoryBase<ProductionCompany>
    {
        // Get all production companies sorted by name
        IEnumerable<ProductionCompany> GetAllSorted();
    }
} 