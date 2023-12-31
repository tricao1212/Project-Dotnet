﻿using BookStore.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BookStore.Models
{
    
    public class Profile
    {
        [Key]
        public int UserId { get; set; }

        public string Avatar { get; set; }
        public BookUser User { get; set; }

        [MaxLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

		[MaxLength(100)]
		[Display(Name = "Last name")]
		public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string Address { get; set; }

        [Display(Name = "Phone number")]
        [RegularExpression("[0-9]{10}")]
        public string PhoneNumber { get; set; }
        [ForeignKey(nameof(Rank))]
        public int RankId { get; set; }
        public Rank Rank { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0;
    }
}
