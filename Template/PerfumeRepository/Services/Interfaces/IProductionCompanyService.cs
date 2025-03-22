using PerfumeRepository.Models;
using System.Collections.Generic;

namespace PerfumeRepository.Services.Interfaces
{
    public interface IProductionCompanyService
    {
        // Get all production companies 
        IEnumerable<ProductionCompany> GetAllCompanies();
        
        // Get a specific company by ID
        ProductionCompany GetCompanyById(string id);
    }
} 