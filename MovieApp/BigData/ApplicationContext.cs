using MoviesData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Actor> Actors => Set<Actor>();
        public DbSet<Tag> Tags => Set<Tag>();
        public ApplicationContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>()
                    .HasMany(m => m.Actors)
                    .WithMany(a => a.Movies);
            modelBuilder.Entity<Movie>()
                    .HasMany(m => m.Tags)
                    .WithMany(a => a.Movies);
        }
    }
}
