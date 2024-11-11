
using Microsoft.AspNetCore.Identity;

namespace IotCloudServices.Common.Models
{
    using UserDbIndexType = System.Int32;        
    public class ApplicationUser : IdentityUser<UserDbIndexType>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

    }

}
