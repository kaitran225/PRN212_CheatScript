using System.Collections.Generic;
using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public interface IProductionCompanyRepository
    {
        List<ProductionCompany> GetAll();
        ProductionCompany GetById(string id);
        void Add(ProductionCompany entity);
        void Update(ProductionCompany entity);
        void Delete(string id);
        List<ProductionCompany>? GetAllSorted();
    }
}
