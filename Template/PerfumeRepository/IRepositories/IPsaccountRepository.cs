using PerfumeRepository.Models;

namespace PerfumeRepository.IRepositories
{
    public interface IPsaccountRepository : IRepositoryBase<Psaccount>
    {
        // Authenticate user by email and password
        Psaccount Authenticate(string email, string password);
        
        // Get user by email
        Psaccount GetByEmail(string email);
    }
} 