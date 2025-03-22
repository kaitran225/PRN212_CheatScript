using System;
using System.Collections.Generic;

namespace PerfumeManagement_SE172279_BLL.Models;

public partial class ProductionCompany
{
    public string ProductionCompanyId { get; set; } = null!;

    public string ProductionCompanyName { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string ProductionCompanyAddress { get; set; } = null!;

    public virtual ICollection<PerfumeInformation> PerfumeInformations { get; set; } = [];
}
