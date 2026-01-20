using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IConfigurationService
    {
        string GetApiUrl(string serviceName);
        string GetApiPath(string serviceName, string pathName);
        string GetAssetUrl();
        string GetGoogleMapsApiKey();
    }
}
