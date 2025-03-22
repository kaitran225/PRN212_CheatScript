using PerfumeRepository.DTOs;
using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using PerfumeRepository.Services.Interfaces;

namespace PerfumeRepository.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public UserDTO Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return new UserDTO { IsAuthenticated = false };
            }

            Psaccount user = _unitOfWork.PsAccount.Authenticate(email, password);

            // Check if user exists and has valid role
            if (user == null || !(user.Role == 2 || user.Role == 3))
            {
                return new UserDTO { IsAuthenticated = false };
            }

            // Create user DTO with authenticated status
            return new UserDTO
            {
                UserId = user.PsaccountId,
                Email = user.EmailAddress!,
                Note = user.PsaccountNote,
                Role = user.Role,
                IsAuthenticated = true
            };
        }

        public bool IsAuthorizedForCrud(UserDTO user)
        {
            return user != null && user.IsAuthenticated && user.IsManager;
        }

        public bool IsAuthorizedForRead(UserDTO user)
        {
            return user != null && user.IsAuthenticated && (user.IsManager || user.IsStaff);
        }
    }
} 