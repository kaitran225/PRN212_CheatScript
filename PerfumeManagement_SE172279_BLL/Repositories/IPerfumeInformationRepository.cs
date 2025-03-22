using System.Collections.Generic;
using System.Linq.Expressions;
using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public interface IPerfumeInformationRepository
    {
        List<PerfumeInformation> GetAll();
        PerfumeInformation GetById(string id);
        void Add(PerfumeInformation entity);
        void Update(PerfumeInformation entity);
        List<PerfumeInformation>? Find(Expression<Func<PerfumeInformation, bool>> expression);
        void Delete(string id);
    }
}
