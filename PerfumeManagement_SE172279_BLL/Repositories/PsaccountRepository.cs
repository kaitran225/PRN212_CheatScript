using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public class PsaccountRepository : IPsaccountRepository
    {
        private readonly PerfumeContext? _dbContext;

        public PsaccountRepository(PerfumeContext context)
        {
            _dbContext = context;
        }

        public List<Psaccount> GetAll()
        {
            return _dbContext!.Set<Psaccount>().ToList();
        }

        public Psaccount GetById(string id)
        {
            return _dbContext!.Set<Psaccount>().Find(id)!;
        }

        public void Add(Psaccount entity)
        {
            _dbContext?.Set<Psaccount>().Add(entity);
            _dbContext?.SaveChanges();
        }

        public void Update(Psaccount entity)
        {
            _dbContext?.Set<Psaccount>().Update(entity);
            _dbContext?.SaveChanges();
        }

        public void Delete(string id)
        {
            Psaccount? entity = _dbContext!.Set<Psaccount>().Find(id);
            if (entity != null)
            {
                _dbContext?.Set<Psaccount>().Remove(entity);
                _dbContext?.SaveChanges();
            }
        }
        public Psaccount? Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            return _dbContext!.Psaccounts
                .FirstOrDefault(a => a.EmailAddress != null &&
                                   a.EmailAddress.ToLower() == email.ToLower() &&
                                   a.Password == password);
        }
         
        public Psaccount? GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return _dbContext!.Psaccounts
                .FirstOrDefault(a => a.EmailAddress != null &&
                                   a.EmailAddress.ToLower() == email.ToLower());
        }
    }
}
