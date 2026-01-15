using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface ICatergoryMasterService
    {
        Task<List<CategoryMasterModel>> GetCategoryMastersAsync();
    }
}
 