using System;

namespace PerfumeManagement_SE172279_DAL.DTO
{
    public class PerfumeDTO
    {
        public string? PerfumeId { get; set; }
        public string? PerfumeName { get; set; }
        public string? Ingredients { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Concentration { get; set; }
        public string? Longevity { get; set; }
        public string? ProductionCompanyId { get; set; }
        public string? ProductionCompanyName { get; set; }
    }
} 