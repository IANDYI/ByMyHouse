using BuyMyHouse.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BuyMyHouse.Core.Data;

public class HouseDbContext: DbContext
{
    public HouseDbContext(DbContextOptions<HouseDbContext> options) : base(options)
    {
    }

    public DbSet<House> Houses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<House>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Bedrooms).HasColumnType("int");
            entity.Property(e => e.Bathrooms).HasColumnType("int");
            entity.Property(e => e.SquareMeters).HasColumnType("int");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(stringListComparer);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.City);
        });

        modelBuilder.Entity<House>().HasData(
            new House
            {
                Id = 1,
                Address = "Prinsengracht 112",
                City = "Amsterdam",
                Price = 520000m,
                Bedrooms = 2,
                Bathrooms = 1,
                SquareMeters = 85,
                Description = "Charming traditional Amsterdam house with original wooden beams. Situated on the famous canal with garden access. Perfect for those seeking authentic Dutch living in the Jordaan district.",
                ListedDate = new DateTime(2025, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                IsAvailable = true
            },
            new House
            {
                Id = 2,
                Address = "Langestraat 45B",
                City = "Alkmaar",
                Price = 420000m,
                Bedrooms = 3,
                Bathrooms = 2,
                SquareMeters = 110,
                Description = "Contemporary townhouse with open-plan living area and private terrace. Modern finishes throughout, recently renovated. Located in historic city center close to shops and restaurants. Close to train station with excellent connections to Amsterdam.",
                ListedDate = new DateTime(2025, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                IsAvailable = true
            }
        );
    }
}
