using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using PerfumeRepository.Services.Interfaces;
using System.Collections.Generic;

namespace PerfumeRepository.Services
{
    public class ProductionCompanyService : IProductionCompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductionCompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ProductionCompany> GetAllCompanies()
        {
            return _unitOfWork.ProductionCompany.GetAllSorted();
        }

        public ProductionCompany GetCompanyById(string id)
        {
            return _unitOfWork.ProductionCompany.GetById(id);
        }
    }
} 