using Microsoft.EntityFrameworkCore;
using NorthWind.DataAccessLayer.Entities;
using NorthWind_DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ShoppingBag> ShoppingBags { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<MemberOrder> MemberOrders { get; set; }
        public DbSet<MemberOrderProduct> MemberOrderProducts { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShoppingBag>()
                .HasKey(sb => sb.ID);

            // Configure the relationships
            modelBuilder.Entity<ShoppingBag>()
                .HasOne(sb => sb.Product)
                .WithMany() 
                .HasForeignKey(sb => sb.ProductID);

            modelBuilder.Entity<ShoppingBag>()
                .HasOne(sb => sb.Member)
                .WithMany() 
                .HasForeignKey(sb => sb.UsernameID);
        }

    }
}
