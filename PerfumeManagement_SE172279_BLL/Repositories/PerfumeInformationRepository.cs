using Microsoft.EntityFrameworkCore;
using PerfumeManagement_SE172279_BLL.Models ;
using System.Linq.Expressions;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public class PerfumeInformationRepository : IPerfumeInformationRepository
    {
        private readonly PerfumeContext? _dbContext;

        public PerfumeInformationRepository() { }

        public PerfumeInformationRepository(PerfumeContext context)
        {
            _dbContext = context;
        }

        public List<PerfumeInformation> GetAll()
        {
            return [.. _dbContext!.PerfumeInformations];
        }

        public PerfumeInformation GetById(string id)
        {
            return _dbContext!.PerfumeInformations.Find(id)!;
        }

        public void Add(PerfumeInformation entity)
        {
            _dbContext?.PerfumeInformations.Add(entity);
            _dbContext?.SaveChanges();
        }
        public List<PerfumeInformation>? Find(Expression<Func<PerfumeInformation, bool>> expression)
        {
            return _dbContext!.PerfumeInformations.Where(expression).ToList()!;
        }

        public void Update(PerfumeInformation entity)
        {
            _dbContext?.PerfumeInformations.Update(entity);
            _dbContext?.SaveChanges();
        }

        public void Delete(string id)
        {
            PerfumeInformation? entity = _dbContext!.PerfumeInformations.Find(id);
            if (entity != null)
            {
                _dbContext?.PerfumeInformations.Remove(entity);
                _dbContext?.SaveChanges();
            }
        }
        public IEnumerable<PerfumeInformation>? GetPerfumesWithCompany()
        {
            return _dbContext?.PerfumeInformations
                .Include(p => p.ProductionCompany)
                .ToList();
        }

        public IEnumerable<IGrouping<string, PerfumeInformation>>? GroupByIngredients()
        {
            return _dbContext?.PerfumeInformations
                .AsEnumerable()
                .GroupBy(p => p.Ingredients);
        }

        public IEnumerable<PerfumeInformation>? SearchPerfumes(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetAll();
            return _dbContext?.PerfumeInformations
                .Include(p => p.ProductionCompany)
                .Where(p => p.Ingredients.Contains(searchTerm) ||
                            p.Concentration.Contains(searchTerm))
                .ToList();
        }
    }
}
