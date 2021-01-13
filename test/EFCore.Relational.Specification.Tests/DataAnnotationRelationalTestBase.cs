// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class DataAnnotationRelationalTestBase<TFixture> : DataAnnotationTestBase<TFixture>
        where TFixture : DataAnnotationRelationalTestBase<TFixture>.DataAnnotationRelationalFixtureBase, new()
    {
        protected DataAnnotationRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Table_can_configure_TPT_with_Owned()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var model = context.Model;

                    var animalType = model.FindEntityType(typeof(Animal));
                    Assert.Equal("Animals", animalType.GetTableMappings().Single().Table.Name);

                    var petType = model.FindEntityType(typeof(Pet));
                    Assert.Equal("Pets", petType.GetTableMappings().Last().Table.Name);

                    var tagNavigation = petType.FindNavigation(nameof(Pet.Tag));
                    var ownership = tagNavigation.ForeignKey;
                    Assert.True(ownership.IsRequiredDependent);

                    var petTagType = ownership.DeclaringEntityType;
                    Assert.Equal("Pets", petTagType.GetTableMappings().Single().Table.Name);

                    var tagIdProperty = petTagType.FindProperty(nameof(PetTag.TagId));
                    Assert.False(tagIdProperty.IsNullable);
                    Assert.All(tagIdProperty.GetTableColumnMappings(), m => Assert.False(m.Column.IsNullable));

                    var catType = model.FindEntityType(typeof(Cat));
                    Assert.Equal("Cats", catType.GetTableMappings().Last().Table.Name);

                    var dogType = model.FindEntityType(typeof(Dog));
                    Assert.Equal("Dogs", dogType.GetTableMappings().Last().Table.Name);

                    var petFood = new PetFood() { FoodName = "Fish" };
                    context.Add(petFood);

                    context.Add(
                        new Cat { Species = "Felis catus", Tag = new PetTag { TagId = 2 }, FavoritePetFood = petFood });

                    context.SaveChanges();
                },
                context =>
                {
                    var cat = context.Set<Cat>().Single();
                    Assert.Equal("Felis catus", cat.Species);
                    Assert.Equal(2u, cat.Tag.TagId);
                });
        }

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
}
