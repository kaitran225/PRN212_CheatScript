using Microsoft.EntityFrameworkCore;
using PerfumeRepository.IRepositories;
using PerfumeRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfumeRepository.Repositories
{
    public class PerfumeInformationRepository : RepositoryBase<PerfumeInformation>, IPerfumeInformationRepository
    {
        public PerfumeInformationRepository(PerfumeModelsContext context) : base(context)
        {
        }

        public IEnumerable<PerfumeInformation> GetPerfumesWithCompany()
        {
            return _context.PerfumeInformations
                .Include(p => p.ProductionCompany)
                .ToList();
        }

        public IEnumerable<IGrouping<string, PerfumeInformation>> GroupByIngredients()
        {
            return _context.PerfumeInformations
                .AsEnumerable()
                .GroupBy(p => p.Ingredients);
        }

        public IEnumerable<PerfumeInformation> SearchPerfumes(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetAll();

            // Perform relative search by Ingredients or Concentration
            // Don't group here - let the service layer handle grouping
            return _context.PerfumeInformations
                .Include(p => p.ProductionCompany)
                .Where(p => p.Ingredients.Contains(searchTerm) || 
                            p.Concentration.Contains(searchTerm))
                .ToList();
        }
    }
} 