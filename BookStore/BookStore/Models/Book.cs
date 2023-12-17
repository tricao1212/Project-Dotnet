using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ImgURL { get; set; }
        [NotMapped]
        [Display(Name ="Image")]
        public IFormFile imgFile { get; set; }
        public string Description { get; set; }


        [Display(Name = "Public Date")]
        [DataType(DataType.Date)]
        public DateTime PublicDate { get; set; }

        public ICollection<Genre> Genres { get; set; }

        [Required]
        [ForeignKey(nameof(Author))]
        [Display(Name ="Author")]
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        [Required]
        [ForeignKey(nameof(Publisher))]
        [Display(Name ="Publisher")]
        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; }
    }
}
