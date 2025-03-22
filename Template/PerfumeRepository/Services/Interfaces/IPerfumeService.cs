using PerfumeRepository.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfumeRepository.Services.Interfaces
{
    public interface IPerfumeService
    {
        // Get all perfumes with company info
        IEnumerable<PerfumeDTO> GetAllPerfumes();
        
        // Get a specific perfume by ID
        PerfumeDTO GetPerfumeById(string id);
        
        // Search perfumes by ingredients or concentration
        IEnumerable<IGrouping<string, PerfumeDTO>> SearchPerfumes(string searchTerm);
        
        // Add a new perfume
        (bool success, string message) AddPerfume(PerfumeDTO perfume);
        
        // Update an existing perfume
        (bool success, string message) UpdatePerfume(PerfumeDTO perfume);
        
        // Delete a perfume
        bool DeletePerfume(string id);
        
        // Validate a perfume
        (bool isValid, string validationMessage) ValidatePerfume(PerfumeDTO perfume);
    }
} 