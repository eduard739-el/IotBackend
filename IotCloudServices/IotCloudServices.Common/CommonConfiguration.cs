using IotCloudServices.Common.JWT;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotCloudServices.Common
{
    static public class CommonConfiguration
    {
        static public void AddCommonJsonSettings(ConfigurationManager config)
        {
            var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "commonappsettings.json"); 
            config.AddJsonFile(jsonFilePath);
        }
    }
}
