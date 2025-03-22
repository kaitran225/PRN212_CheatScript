using PerfumeRepository.DTOs;
using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using PerfumeRepository.Services.Interfaces;
using PerfumeRepository.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PerfumeRepository.Services
{
    public class PerfumeService : IPerfumeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PerfumeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<PerfumeDTO> GetAllPerfumes()
        {
            var perfumes = _unitOfWork.PerfumeInformation.GetPerfumesWithCompany();
            
            return perfumes.Select(p => new PerfumeDTO
            {
                PerfumeId = p.PerfumeId,
                PerfumeName = p.PerfumeName,
                Ingredients = p.Ingredients,
                ReleaseDate = p.ReleaseDate,
                Concentration = p.Concentration,
                Longevity = p.Longevity,
                ProductionCompanyId = p.ProductionCompanyId,
                ProductionCompanyName = p.ProductionCompany?.ProductionCompanyName
            });
        }

        public PerfumeDTO GetPerfumeById(string id)
        {
            var perfume = _unitOfWork.PerfumeInformation.Find(p => p.PerfumeId == id)
                .FirstOrDefault();
                
            if (perfume == null)
                return null;
                
            var company = _unitOfWork.ProductionCompany.GetById(perfume.ProductionCompanyId);
            
            return new PerfumeDTO
            {
                PerfumeId = perfume.PerfumeId,
                PerfumeName = perfume.PerfumeName,
                Ingredients = perfume.Ingredients,
                ReleaseDate = perfume.ReleaseDate,
                Concentration = perfume.Concentration,
                Longevity = perfume.Longevity,
                ProductionCompanyId = perfume.ProductionCompanyId,
                ProductionCompanyName = company?.ProductionCompanyName
            };
        }

        public IEnumerable<IGrouping<string, PerfumeDTO>> SearchPerfumes(string searchTerm)
        {
            // Get perfumes matching the search term
            var perfumes = _unitOfWork.PerfumeInformation.SearchPerfumes(searchTerm);
            
            // Convert to DTOs
            var perfumeDTOs = perfumes.Select(p => new PerfumeDTO
            {
                PerfumeId = p.PerfumeId,
                PerfumeName = p.PerfumeName,
                Ingredients = p.Ingredients,
                ReleaseDate = p.ReleaseDate,
                Concentration = p.Concentration,
                Longevity = p.Longevity,
                ProductionCompanyId = p.ProductionCompanyId,
                ProductionCompanyName = p.ProductionCompany?.ProductionCompanyName
            }).ToList();
            
            // Group by Ingredients as required
            return perfumeDTOs.GroupBy(p => p.Ingredients);
        }

        public (bool success, string message) AddPerfume(PerfumeDTO perfumeDTO)
        {
            // Validate the perfume
            var validation = ValidatePerfume(perfumeDTO);
            if (!validation.isValid)
            {
                return (false, validation.validationMessage);
            }
            
            // Check if ID already exists
            var existingPerfume = _unitOfWork.PerfumeInformation.GetById(perfumeDTO.PerfumeId);
            if (existingPerfume != null)
            {
                return (false, "A perfume with this ID already exists");
            }
            
            // Create a new perfume entity
            var perfume = new PerfumeInformation
            {
                PerfumeId = perfumeDTO.PerfumeId,
                PerfumeName = perfumeDTO.PerfumeName,
                Ingredients = perfumeDTO.Ingredients,
                ReleaseDate = perfumeDTO.ReleaseDate,
                Concentration = perfumeDTO.Concentration,
                Longevity = perfumeDTO.Longevity,
                ProductionCompanyId = perfumeDTO.ProductionCompanyId
            };
            
            try
            {
                _unitOfWork.PerfumeInformation.Add(perfume);
                _unitOfWork.Complete();
                return (true, "Perfume added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding perfume: {ex.Message}");
            }
        }

        public (bool success, string message) UpdatePerfume(PerfumeDTO perfumeDTO)
        {
            // Validate the perfume
            var validation = ValidatePerfume(perfumeDTO);
            if (!validation.isValid)
            {
                return (false, validation.validationMessage);
            }
            
            // Check if perfume exists
            var existingPerfume = _unitOfWork.PerfumeInformation.GetById(perfumeDTO.PerfumeId);
            if (existingPerfume == null)
            {
                return (false, "Perfume not found");
            }
            
            // Update the existing perfume
            existingPerfume.PerfumeName = perfumeDTO.PerfumeName;
            existingPerfume.Ingredients = perfumeDTO.Ingredients;
            existingPerfume.ReleaseDate = perfumeDTO.ReleaseDate;
            existingPerfume.Concentration = perfumeDTO.Concentration;
            existingPerfume.Longevity = perfumeDTO.Longevity;
            existingPerfume.ProductionCompanyId = perfumeDTO.ProductionCompanyId;
            
            try
            {
                _unitOfWork.PerfumeInformation.Update(existingPerfume);
                _unitOfWork.Complete();
                return (true, "Perfume updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating perfume: {ex.Message}");
            }
        }

        public bool DeletePerfume(string id)
        {
            var perfume = _unitOfWork.PerfumeInformation.GetById(id);
            
            if (perfume == null)
                return false;
                
            _unitOfWork.PerfumeInformation.Delete(perfume);
            _unitOfWork.Complete();
            
            return true;
        }

        public (bool isValid, string validationMessage) ValidatePerfume(PerfumeDTO perfume)
        {
            // Check for null/empty fields
            if (string.IsNullOrEmpty(perfume.PerfumeId))
                return (false, "Perfume ID is required");
                
            if (string.IsNullOrEmpty(perfume.PerfumeName))
                return (false, "Perfume Name is required");
                
            if (string.IsNullOrEmpty(perfume.Ingredients))
                return (false, "Ingredients are required");
                
            if (string.IsNullOrEmpty(perfume.Concentration))
                return (false, "Concentration is required");
                
            if (string.IsNullOrEmpty(perfume.Longevity))
                return (false, "Longevity is required");
                
            if (string.IsNullOrEmpty(perfume.ProductionCompanyId))
                return (false, "Production Company is required");
            
            // Validate perfume name length
            if (!ValidationHelper.ValidateLength(perfume.PerfumeName, 5, 90))
                return (false, "Perfume Name must be between 5 and 90 characters");
            
            // Check for special characters
            if (ValidationHelper.ContainsSpecialCharacters(perfume.PerfumeName))
                return (false, "Perfume Name must not contain special characters such as $, %, ^, @");
            
            // Check each word starts with capital letter or digit
            if (!ValidationHelper.ValidateWordCapitalization(perfume.PerfumeName))
                return (false, "Each word of Perfume Name must begin with a capital letter or digits 1-9");
            
            return (true, string.Empty);
        }
    }
} 