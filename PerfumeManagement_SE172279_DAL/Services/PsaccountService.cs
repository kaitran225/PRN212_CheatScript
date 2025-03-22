using PerfumeManagement_SE172279_BLL.Models;
using PerfumeManagement_SE172279_BLL.Repositories;
using PerfumeManagement_SE172279_DAL.DTO;

namespace PerfumeManagement_SE172279_DAL.Services
{
    public class PsaccountService
    {
        private readonly PsaccountRepository _repo;
        private readonly PerfumeContext _context;

        public PsaccountService()
        {
            _context = new PerfumeContext();
            _repo = new PsaccountRepository(_context);
        }

        public List<Psaccount> GetAll()
        {
            return _repo.GetAll();
        }

        public Psaccount? GetById(string id)
        {
            return _repo.GetById(id);
        }

        public void Add(Psaccount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Psaccount cannot be null.");
            
            _repo.Add(entity);
        }

        public void Update(Psaccount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Psaccount cannot be null.");

            _repo.Update(entity);
        }

        public void Delete(string id)
        {
            _repo.Delete(id);
        }

        public List<Psaccount> Search(string searchTerm)
        {
            List<Psaccount> entities = _repo.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return entities.Where(x => x.ToString()!.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            return entities;
        }
        public UserDTO Login(string email, string password)
        {

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return new UserDTO { IsAuthenticated = false };
            }

            Psaccount user = _repo.Authenticate(email, password)!;

            if (user == null || !(user.Role == 2 || user.Role == 3))
            {
                return new UserDTO { IsAuthenticated = false };
            }

            return new UserDTO
            {
                UserId = user.PsaccountId,
                Email = user.EmailAddress!,
                Note = user.PsaccountNote,
                Role = user.Role,
                IsAuthenticated = true
            };
        }

        // Only Admin
        public bool IsAuthorizedForCrud(UserDTO user)
        {
            return user != null && user.IsAuthenticated && user.IsManager;
        }

        // For staff
        public bool IsAuthorizedForRead(UserDTO user)
        {
            return user != null && user.IsAuthenticated && (user.IsManager || user.IsStaff);
        }
    }
}
