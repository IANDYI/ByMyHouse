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
            entity.Property(e => e.ListingPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.BedroomCount).HasColumnType("int");
            entity.Property(e => e.BathroomCount).HasColumnType("int");
            entity.Property(e => e.AreaInSquareMeters).HasColumnType("int");
            entity.Property(e => e.PropertyDescription).HasMaxLength(1000);
            entity.Property(e => e.PictureUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(stringListComparer);
            entity.HasIndex(e => e.ListingPrice);
            entity.HasIndex(e => e.LocationCity);
        });

        modelBuilder.Entity<House>().HasData(
            new House
            {
                Id = 1,
                PropertyAddress = "Prinsengracht 112",
                LocationCity = "Amsterdam",
                ListingPrice = 520000m,
                BedroomCount = 2,
                BathroomCount = 1,
                AreaInSquareMeters = 85,
                PropertyDescription = "Charming traditional Amsterdam house with original wooden beams. Situated on the famous canal with garden access. Perfect for those seeking authentic Dutch living in the Jordaan district.",
                DateListed = new DateTime(2025, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                CurrentlyAvailable = true
            },
            new House
            {
                Id = 2,
                PropertyAddress = "Langestraat 45B",
                LocationCity = "Alkmaar",
                ListingPrice = 420000m,
                BedroomCount = 3,
                BathroomCount = 2,
                AreaInSquareMeters = 110,
                PropertyDescription = "Contemporary townhouse with open-plan living area and private terrace. Modern finishes throughout, recently renovated. Located in historic city center close to shops and restaurants. Close to train station with excellent connections to Amsterdam.",
                DateListed = new DateTime(2025, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                CurrentlyAvailable = true
            }
        );
    }
}
