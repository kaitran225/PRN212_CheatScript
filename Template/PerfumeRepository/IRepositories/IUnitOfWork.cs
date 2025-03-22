using System;

namespace PerfumeRepository.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IPerfumeInformationRepository PerfumeInformation { get; }
        IProductionCompanyRepository ProductionCompany { get; }
        IPsaccountRepository PsAccount { get; }
        
        int Complete();
    }
} 