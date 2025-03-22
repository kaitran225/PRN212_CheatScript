using Microsoft.EntityFrameworkCore;
using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public class ProductionCompanyRepository : IProductionCompanyRepository
    {
        private readonly PerfumeContext? _dbContext;

        public ProductionCompanyRepository() { }

        public ProductionCompanyRepository(PerfumeContext context)
        {
            _dbContext = context;
        }

        public List<ProductionCompany> GetAll()
        {
            return [.. _dbContext!.Set<ProductionCompany>()];
        }

        public ProductionCompany GetById(string id)
        {
            return _dbContext!.Set<ProductionCompany>().Find(id)!;
        }

        public void Add(ProductionCompany entity)
        {
            _dbContext?.Set<ProductionCompany>().Add(entity);
            _dbContext?.SaveChanges();
        }

        public void Update(ProductionCompany entity)
        {
            _dbContext?.Set<ProductionCompany>().Update(entity);
            _dbContext?.SaveChanges();
        }
        public List<ProductionCompany>? GetAllSorted()
        {
            return _dbContext?.ProductionCompanies
                .OrderBy(p => p.ProductionCompanyName)
                .ToList();
        }
        public void Delete(string id)
        {
            ProductionCompany? entity = _dbContext!.Set<ProductionCompany>().Find(id);
            if (entity != null)
            {
                _dbContext?.Set<ProductionCompany>().Remove(entity);
                _dbContext?.SaveChanges();
            }
        }
    }
}
