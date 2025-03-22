using System.Collections.Generic;
using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279_BLL.Repositories
{
    public interface IPsaccountRepository
    {
        List<Psaccount> GetAll();
        Psaccount GetById(string id);
        void Add(Psaccount entity);
        void Update(Psaccount entity);
        void Delete(string id);
    }
}
