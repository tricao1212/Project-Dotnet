using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Data
{
    public class BookStoreContext : IdentityDbContext<BookUser, IdentityRole<int>, int>
    {
        public BookStoreContext (DbContextOptions<BookStoreContext> options)
            : base(options)
        {
        }

        public DbSet<BookStore.Models.Book> Book { get; set; } = default!;

        public DbSet<BookStore.Models.Author> Author { get; set; }

        public DbSet<BookStore.Models.Genre> Genre { get; set; }

        public DbSet<BookStore.Models.Publisher> Publisher { get; set; }

        public DbSet<BookStore.Models.Coupon> Coupon { get; set; }

        public DbSet<BookStore.Models.Profile> Profile { get; set; }

        public DbSet<BookStore.Models.Order> Order { get; set; }

        public DbSet<BookStore.Models.Bill> Bill { get; set; }
        public DbSet<BookStore.Models.Rank> Ranks { get; set; }
    }
}
