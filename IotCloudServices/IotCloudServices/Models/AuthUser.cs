global using UserDbIndexType = System.Int32;

namespace IotCloudServices.Services.Authentication.Models
{
    public class AuthUser
    {        
        public UserDbIndexType Id { get ; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

}
