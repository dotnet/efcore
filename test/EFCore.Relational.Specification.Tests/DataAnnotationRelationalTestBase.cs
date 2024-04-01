// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class DataAnnotationRelationalTestBase<TFixture> : DataAnnotationTestBase<TFixture>
    where TFixture : DataAnnotationRelationalTestBase<TFixture>.DataAnnotationRelationalFixtureBase, new()
{
    protected DataAnnotationRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void ForeignKey_to_ForeignKey_on_many_to_many()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Login16>(
            entity =>
            {
                entity.HasMany(d => d.Profile16s)
                    .WithMany(p => p.Login16s)
                    .UsingEntity<Dictionary<string, object>>(
                        "Login16Profile16",
                        l => l.HasOne<Profile16>().WithMany().HasForeignKey("Profile16Id"),
                        r => r.HasOne<Login16>().WithMany().HasForeignKey("Login16Id"),
                        j =>
                        {
                            j.HasKey("Login16Id", "Profile16Id");

                            j.ToTable("Login16Profile16");
                        });
            });

        var model = Validate(modelBuilder);

        var login = modelBuilder.Model.FindEntityType(typeof(Login16));
        var logins = login.FindSkipNavigation(nameof(Login16.Profile16s));
        var join = logins.JoinEntityType;
        Assert.Equal(2, join.GetProperties().Count());
        Assert.False(GetProperty<Login16>(model, "Login16Id").IsForeignKey());
        Assert.False(GetProperty<Profile16>(model, "Profile16Id").IsForeignKey());
    }

    public class Login16
    {
        public int Login16Id { get; set; }

        [ForeignKey("Login16Id")]
        public virtual ICollection<Profile16> Profile16s { get; set; }
    }

    public class Profile16
    {
        public int Profile16Id { get; set; }

        [ForeignKey("Profile16Id")]
        public virtual ICollection<Login16> Login16s { get; set; }
    }

    [ConditionalFact]
    public virtual Task Table_can_configure_TPT_with_Owned()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var model = context.Model;

                var animalType = model.FindEntityType(typeof(Animal))!;
                Assert.Equal("Animals", animalType.GetTableMappings().Single().Table.Name);

                var petType = model.FindEntityType(typeof(Pet))!;
                Assert.Equal("Pets", petType.GetTableMappings().Last().Table.Name);

                var tagNavigation = petType.FindNavigation(nameof(Pet.Tag))!;
                var ownership = tagNavigation.ForeignKey;
                Assert.True(ownership.IsRequiredDependent);

                var petTagType = ownership.DeclaringEntityType;
                Assert.Equal("Pets", petTagType.GetTableMappings().Single().Table.Name);

                var tagIdProperty = petTagType.FindProperty(nameof(PetTag.TagId))!;
                Assert.False(tagIdProperty.IsNullable);
                Assert.All(tagIdProperty.GetTableColumnMappings(), m => Assert.False(m.Column.IsNullable));

                var catType = model.FindEntityType(typeof(Cat))!;
                Assert.Equal("Cats", catType.GetTableMappings().Last().Table.Name);

                var dogType = model.FindEntityType(typeof(Dog))!;
                Assert.Equal("Dogs", dogType.GetTableMappings().Last().Table.Name);

                var petFood = new PetFood { FoodName = "Fish" };
                context.Add(petFood);

                context.Add(
                    new Cat
                    {
                        Species = "Felis catus",
                        Tag = new PetTag { TagId = 2 },
                        FavoritePetFood = petFood
                    });

                return context.SaveChangesAsync();
            }, async context =>
            {
                var cat = await context.Set<Cat>().SingleAsync();
                Assert.Equal("Felis catus", cat.Species);
                Assert.Equal(2u, cat.Tag.TagId);
            });

    public abstract class DataAnnotationRelationalFixtureBase : DataAnnotationFixtureBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Pet>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();
        }
    }

    [Table("Animals")]
    protected class Animal
    {
        [Key]
        public int Key { get; set; }

        public string Species { get; set; }
    }

    [Table("Pets")]
    protected class Pet : Animal
    {
        public string Name { get; set; }

        [Column("FavoritePetFood_Id")]
        [ForeignKey(nameof(FavoritePetFood))]
        public int? FavoritePetFoodId { get; set; }

        public PetFood FavoritePetFood { get; set; }

        [Required]
        public PetTag Tag { get; set; }
    }

    [Table("Cats")]
    protected sealed class Cat : Pet
    {
        public string EducationLevel { get; set; }
    }

    [Table("Dogs")]
    protected sealed class Dog : Pet
    {
        public string FavoriteToy { get; set; }
    }

    [Owned]
    protected sealed class PetTag
    {
        [Required]
        public uint? TagId { get; set; }
    }

    [Table("PetFoods")]
    public sealed class PetFood
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("PetFoods_Id")]
        public int PetFoodId { get; set; }

        public string FoodName { get; set; }
    }
}
