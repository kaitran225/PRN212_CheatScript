using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using PerfumeRepository.Repositories;
using PerfumeRepository.Services.Interfaces;
using System;

namespace PerfumeRepository.Services
{
    public class ServiceProvider : IDisposable
    {
        private readonly PerfumeModelsContext _context;
        private readonly IUnitOfWork _unitOfWork;
        
        private IAuthService _authService;
        private IPerfumeService _perfumeService;
        private IProductionCompanyService _productionCompanyService;
        
        public ServiceProvider()
        {
            _context = new PerfumeModelsContext();
            _unitOfWork = new UnitOfWork(_context);
        }
        
        public IAuthService AuthService
        {
            get
            {
                if (_authService == null)
                {
                    _authService = new AuthService(_unitOfWork);
                }
                return _authService;
            }
        }
        
        public IPerfumeService PerfumeService
        {
            get
            {
                if (_perfumeService == null)
                {
                    _perfumeService = new PerfumeService(_unitOfWork);
                }
                return _perfumeService;
            }
        }
        
        public IProductionCompanyService ProductionCompanyService
        {
            get
            {
                if (_productionCompanyService == null)
                {
                    _productionCompanyService = new ProductionCompanyService(_unitOfWork);
                }
                return _productionCompanyService;
            }
        }
        
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
} 