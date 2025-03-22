using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using System;

namespace PerfumeRepository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PerfumeModelsContext _context;
        private IPerfumeInformationRepository _perfumeInformation;
        private IProductionCompanyRepository _productionCompany;
        private IPsaccountRepository _psAccount;

        public UnitOfWork(PerfumeModelsContext context)
        {
            _context = context;
        }

        public IPerfumeInformationRepository PerfumeInformation
        {
            get
            {
                if (_perfumeInformation == null)
                {
                    _perfumeInformation = new PerfumeInformationRepository(_context);
                }
                return _perfumeInformation;
            }
        }

        public IProductionCompanyRepository ProductionCompany
        {
            get
            {
                if (_productionCompany == null)
                {
                    _productionCompany = new ProductionCompanyRepository(_context);
                }
                return _productionCompany;
            }
        }

        public IPsaccountRepository PsAccount
        {
            get
            {
                if (_psAccount == null)
                {
                    _psAccount = new PsaccountRepository(_context);
                }
                return _psAccount;
            }
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 