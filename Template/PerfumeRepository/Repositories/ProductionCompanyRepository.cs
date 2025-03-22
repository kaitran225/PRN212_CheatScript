using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using System.Collections.Generic;
using System.Linq;

namespace PerfumeRepository.Repositories
{
    public class ProductionCompanyRepository : RepositoryBase<ProductionCompany>, IProductionCompanyRepository
    {
        public ProductionCompanyRepository(PerfumeModelsContext context) : base(context)
        {
        }

        public IEnumerable<ProductionCompany> GetAllSorted()
        {
            return _context.ProductionCompanies
                .OrderBy(p => p.ProductionCompanyName)
                .ToList();
        }
    }
} 