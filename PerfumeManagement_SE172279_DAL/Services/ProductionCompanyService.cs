using PerfumeManagement_SE172279_BLL.Models;
using PerfumeManagement_SE172279_BLL.Repositories;

namespace PerfumeManagement_SE172279_DAL.Services
{
    public class ProductionCompanyService
    {
        private readonly ProductionCompanyRepository _repo;
        private readonly PerfumeContext _context;

        public ProductionCompanyService()
        {
            _context = new PerfumeContext();
            _repo = new ProductionCompanyRepository(_context);
        }

        public List<ProductionCompany> GetAll()
        {
            return _repo.GetAll();
        }

        public ProductionCompany? GetById(string id)
        {
            return _repo.GetById(id);
        }

        public void Add(ProductionCompany entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "ProductionCompany cannot be null.");
            
            _repo.Add(entity);
        }

        public void Update(ProductionCompany entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "ProductionCompany cannot be null.");

            _repo.Update(entity);
        }

        public void Delete(string id)
        {
            _repo.Delete(id);
        }

        public List<ProductionCompany> Search(string searchTerm)
        {
            List<ProductionCompany> entities = _repo.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return entities.Where(x => x.ToString()!.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            return entities;
        }
        public IEnumerable<ProductionCompany> GetAllCompanies()
        {
            return _repo.GetAllSorted()!;
        }

        public ProductionCompany GetCompanyById(string id)
        {
            return _repo.GetById(id);
        }
    }
}
