using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ConnectMagar.Models;

namespace ConnectMagar.Data
{
    public class ConnectMagarContext : DbContext
    {
       public ConnectMagarContext(DbContextOptions<ConnectMagarContext> options) : base(options)
        {
            
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<Address>().ToTable("Address");
        }
    }
}