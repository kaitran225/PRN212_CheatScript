using PerfumeRepository.Models;
using System.Collections.Generic;
using System.Linq;

namespace PerfumeRepository.IRepositories
{
    public interface IPerfumeInformationRepository : IRepositoryBase<PerfumeInformation>
    {
        // Get perfumes with production company information
        IEnumerable<PerfumeInformation> GetPerfumesWithCompany();
        
        // Search perfumes by ingredients or concentration
        IEnumerable<PerfumeInformation> SearchPerfumes(string searchTerm);
        
        // Group perfumes by ingredients
        IEnumerable<IGrouping<string, PerfumeInformation>> GroupByIngredients();
    }
} 