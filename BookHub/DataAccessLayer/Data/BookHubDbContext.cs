using System.Linq.Expressions;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Data;

public class BookHubDbContext : DbContext
{
    public BookHubDbContext(DbContextOptions<BookHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<WishList> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<User> Users { get; set; }
    
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.SetNull;

        // Global query filter to exclude soft-deleted entities (IsRemoved = true) from all queries 
        // for selection of the removed entities, use IgnoreQueryFilters() in the query should work hopefully
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(BaseEntity.IsRemoved)),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Author>()
            .HasIndex(a => a.Name);

        modelBuilder.Entity<Genre>()
            .HasIndex(g => g.Name);

        modelBuilder.Entity<Publisher>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Name);

        modelBuilder.Entity<Book>()
            .HasMany(a => a.Authors)
            .WithMany(a => a.Books)
            .UsingEntity<Dictionary<string, object>>(
                "BookAuthor",
                j => j.HasOne<Author>().WithMany().HasForeignKey("AuthorsId"),
                j => j.HasOne<Book>().WithMany().HasForeignKey("BooksId")
            );

        modelBuilder.Entity<Book>()
            .HasMany(a => a.Genres)
            .WithMany(a => a.Books)
            .UsingEntity<Dictionary<string, object>>(
                "BookGenre",
                j => j.HasOne<Genre>().WithMany().HasForeignKey("GenresId"),
                j => j.HasOne<Book>().WithMany().HasForeignKey("BooksId")
            );
        modelBuilder.Entity<Book>()
            .HasOne(b => b.PrimaryGenre)
            .WithMany()
            .HasForeignKey(b => b.PrimaryGenreId);
        
        modelBuilder.Entity<Order>().
            HasOne( o => o.Coupon)
            .WithMany()
            .HasForeignKey( o => o.CouponId);
        modelBuilder.Seed();
        base.OnModelCreating(modelBuilder);
    }
}