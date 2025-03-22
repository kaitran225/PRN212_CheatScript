using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using System.Linq;

namespace PerfumeRepository.Repositories
{
    public class PsaccountRepository : RepositoryBase<Psaccount>, IPsaccountRepository
    {
        public PsaccountRepository(PerfumeModelsContext context) : base(context)
        {
        }

        public Psaccount Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            // Use case-insensitive comparison for email and exact match for password
            return _context.Psaccounts
                .FirstOrDefault(a => a.EmailAddress != null && 
                                   a.EmailAddress.ToLower() == email.ToLower() && 
                                   a.Password == password);
        }

        public Psaccount GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
                
            return _context.Psaccounts
                .FirstOrDefault(a => a.EmailAddress != null && 
                                   a.EmailAddress.ToLower() == email.ToLower());
        }
    }
} 