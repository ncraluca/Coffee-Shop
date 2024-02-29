using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProiectV1.Models;

namespace ProiectV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ProductShoppingCart> ProductShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<ProductFromOrder> ProductFromOrders { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<Address> Adresses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //definire primary key compus
            modelBuilder.Entity<ProductShoppingCart>()
                    .HasKey(pc => new {pc.Id, pc.ProductId, pc.ShoppingCartId });

            //definire relatii cu modelele Product si ShoppingCart
            modelBuilder.Entity<ProductShoppingCart>()
                .HasOne(pc => pc.Product)
                .WithMany(pc => pc.ProductShoppingCarts)
                .HasForeignKey(pc => pc.ProductId);
            modelBuilder.Entity<ProductShoppingCart>()
                .HasOne(pc => pc.ShoppingCart)
                .WithMany(pc => pc.ProductShoppingCarts)
                .HasForeignKey(pc => pc.ShoppingCartId);
            base.OnModelCreating(modelBuilder);

            //relatia many-to-many intre bookmark si article
            // definire primary key compus
            modelBuilder.Entity<ProductFromOrder>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.OrderId });


            // definire relatii cu modelele Bookmark si Article (FK)

            modelBuilder.Entity<ProductFromOrder>()
                .HasOne(ab => ab.Product)
                .WithMany(ab => ab.ProductFromOrders)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<ProductFromOrder>()
                .HasOne(ab => ab.Order)
                .WithMany(ab => ab.ProductFromOrders)
                .HasForeignKey(ab => ab.OrderId);
        }
        
    }
}
