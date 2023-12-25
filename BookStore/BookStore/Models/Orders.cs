using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Orders
    {
        public int Id { get; set; }

        [ForeignKey(nameof(BookUser))]
        public int UserId {  get; set; }
        public BookUser User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


		[Display(Name = "User Name")]
		public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public string Address { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        public string Phone { get; set; }
        public string Note {  get; set; }

		[ForeignKey(nameof(Cart))]
		public int CartId {  get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
