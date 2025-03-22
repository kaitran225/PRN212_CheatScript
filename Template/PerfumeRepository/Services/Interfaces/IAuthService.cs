using PerfumeRepository.DTOs;

namespace PerfumeRepository.Services.Interfaces
{
    public interface IAuthService
    {
        // Authenticate a user by email and password
        UserDTO Login(string email, string password);
        
        // Check if a user is authorized for specific operations
        bool IsAuthorizedForCrud(UserDTO user);
        bool IsAuthorizedForRead(UserDTO user);
    }
} 