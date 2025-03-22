using PerfumeManagement_SE172279_BLL.Models;
using PerfumeManagement_SE172279_BLL.Repositories;
using PerfumeManagement_SE172279_DAL.DTO;
using System.Reflection;

namespace PerfumeManagement_SE172279_DAL.Services
{
    public class PerfumeInformationService
    {
        private readonly PerfumeInformationRepository _repo;
        private readonly ProductionCompanyRepository _companyRepository;
        private readonly PerfumeContext _context;

        public PerfumeInformationService()
        {
            _context = new PerfumeContext();
            _repo = new PerfumeInformationRepository(_context);
            _companyRepository = new ProductionCompanyRepository(_context);
        }

        public List<PerfumeInformation> GetAll()
        {
            return _repo.GetAll();
        }

        public PerfumeInformation? GetById(string id)
        {
            return _repo.GetById(id);
        }

        public void Add(PerfumeInformation entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "PerfumeInformation cannot be null.");
            
            _repo.Add(entity);
        }

        public void Update(PerfumeInformation entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "PerfumeInformation cannot be null.");

            _repo.Update(entity);
        }

        public void Delete(string id)
        {
            _repo.Delete(id);
        }

        public List<PerfumeInformation> Search(string searchTerm)
        {
            List<PerfumeInformation> entities = _repo.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return entities.Where(x => x.ToString()!.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            return entities;
        }
        public IEnumerable<PerfumeDTO>? GetAllPerfumes()
        {
            var perfumes = _repo.GetPerfumesWithCompany();

            return perfumes?.Select(p => new PerfumeDTO
            {
                PerfumeId = p.PerfumeId,
                PerfumeName = p.PerfumeName,
                Ingredients = p.Ingredients,
                ReleaseDate = p.ReleaseDate,
                Concentration = p.Concentration,
                Longevity = p.Longevity,    
                ProductionCompanyId = p.ProductionCompanyId!,
                ProductionCompanyName = p.ProductionCompany!.ProductionCompanyName
            });
        }

        public PerfumeDTO? GetPerfumeById(string id)
        {
            PerfumeInformation? perfume = _repo?.Find(p => p.PerfumeId! == id)
                                              !.FirstOrDefault()!;

            if (perfume == null)
                return null;

            ProductionCompany company = _companyRepository.GetById(perfume.ProductionCompanyId!);

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

        public IEnumerable<IGrouping<string, PerfumeDTO>>? SearchPerfumes(string searchTerm)
        {
            // Get perfumes matching the search term
            var perfumes = _repo.SearchPerfumes(searchTerm);

            // Convert to DTOs
            var perfumeDTOs = perfumes?.Select(p => new PerfumeDTO
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
            return perfumeDTOs?.GroupBy(p => p.Ingredients)!;
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
            var existingPerfume = _repo.GetById(perfumeDTO.PerfumeId!);
            if (existingPerfume != null)
            {
                return (false, "A perfume with this ID already exists");
            }

            // Create a new perfume entity
            var perfume = new PerfumeInformation
            {
                PerfumeId = perfumeDTO.PerfumeId!,
                PerfumeName = perfumeDTO.PerfumeName!,
                Ingredients = perfumeDTO.Ingredients!,
                ReleaseDate = perfumeDTO.ReleaseDate!,
                Concentration = perfumeDTO.Concentration!,
                Longevity = perfumeDTO.Longevity!,
                ProductionCompanyId = perfumeDTO.ProductionCompanyId!
            };

            try
            {
                _repo.Add(perfume);
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
            var existingPerfume = _repo.GetById(perfumeDTO.PerfumeId!);
            if (existingPerfume == null)
            {
                return (false, "Perfume not found");
            }

            // Update the existing perfume
            existingPerfume.PerfumeName = perfumeDTO.PerfumeName!;
            existingPerfume.Ingredients = perfumeDTO.Ingredients!;   
            existingPerfume.ReleaseDate = perfumeDTO.ReleaseDate!;
            existingPerfume.Concentration = perfumeDTO.Concentration!;
            existingPerfume.Longevity = perfumeDTO.Longevity!;
            existingPerfume.ProductionCompanyId = perfumeDTO.ProductionCompanyId!;

            try
            {
                _repo.Update(existingPerfume);
                return (true, "Perfume updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating perfume: {ex.Message}");
            }
        }

        public bool DeletePerfume(string id)
        {
            var perfume = _repo.GetById(id);

            if (perfume == null)
                return false;

            _repo.Delete(id);

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

            return (true, string.Empty);
        }
    }
}
