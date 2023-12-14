using Microsoft.AspNetCore.Identity;
namespace BookStore.Models
{
    public class BookUser:IdentityUser<int>
    {
        public Profile Profile { get; set; }
    }
}
