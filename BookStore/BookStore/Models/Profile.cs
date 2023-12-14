using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Profile
    {
        [Key]
        public int UserId { get; set; }
        public string Avatar { get; set; }
        public BookUser User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public enum Gender { male, female, other }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}
