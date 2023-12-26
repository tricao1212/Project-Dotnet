using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
	public class Rank
	{
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int discount { get; set; } = 0;

        public static void EnsureSeedData(DbContext context)
        {
            if (!context.Set<Rank>().Any())
            {
                context.Set<Rank>().Add(new Rank { Name = "Bronze", discount = 0 });
                context.SaveChanges();
            }
        }
    }
}
