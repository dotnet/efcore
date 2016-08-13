// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal
{
    public class ConventionDispatcherTest
    {
        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IEntityTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.NotNull(b);
                    entityTypeBuilder = new InternalEntityTypeBuilder(b.Metadata, b.ModelBuilder);
                    return entityTypeBuilder;
                });
            conventions.EntityTypeAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.Same(entityTypeBuilder, b);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            if (useBuilder)
            {
                Assert.Null(builder.Entity(typeof(Order), ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(builder.Metadata.AddEntityType(typeof(Order), ConfigurationSource.Convention));
            }

            Assert.NotNull(entityTypeBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnEntityTypeIgnored_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalModelBuilder newModelBuilder = null;
            var convention = new Mock<IEntityTypeIgnoredConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>(), It.IsAny<string>(), It.IsAny<Type>()))
                .Returns<InternalModelBuilder, string, Type>((b, n, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Equal(typeof(Order).DisplayName(), n);
                        Assert.Same(typeof(Order), t);
                        return true;
                    });
            conventions.EntityTypeIgnoredConventions.Add(convention.Object);

            var haltingConvention = new Mock<IEntityTypeIgnoredConvention>();
            haltingConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>(), It.IsAny<string>(), It.IsAny<Type>()))
                .Returns<InternalModelBuilder, string, Type>((b, n, t) =>
                    {
                        newModelBuilder = new InternalModelBuilder(b.Metadata);
                        return false;
                    });
            conventions.EntityTypeIgnoredConventions.Add(haltingConvention.Object);

            var extraConvention = new Mock<IEntityTypeIgnoredConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>(), It.IsAny<string>(), It.IsAny<Type>()))
                .Returns<InternalModelBuilder, string, Type>((b, n, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.EntityTypeIgnoredConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            if (useBuilder)
            {
                builder.Entity(typeof(Order), ConfigurationSource.Convention);
                builder.Ignore(typeof(Order).DisplayName(), ConfigurationSource.Convention);
            }
            else
            {
                builder.Metadata.Ignore(typeof(Order), ConfigurationSource.Convention);
            }

            Assert.NotNull(newModelBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnBaseEntityTypeSet_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IBaseTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Null(t);
                        entityTypeBuilder = b;
                        return true;
                    });
            conventions.BaseEntityTypeSetConventions.Add(convention.Object);

            var nullConvention = new Mock<IBaseTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.Null(t);
                        Assert.Same(entityTypeBuilder, b);
                        return false;
                    });
            conventions.BaseEntityTypeSetConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IBaseTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.BaseEntityTypeSetConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions))
                .Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.HasBaseType(builder.Metadata.Model.AddEntityType(typeof(Order)), ConfigurationSource.Convention);
            }

            Assert.NotNull(entityTypeBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnEntityTypeMemberIgnored_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IEntityTypeMemberIgnoredConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Equal("A", t);
                        entityTypeBuilder = b;
                        return true;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeMemberIgnoredConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.Equal("A", t);
                        Assert.Same(entityTypeBuilder, b);
                        return false;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeMemberIgnoredConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions)).Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.NotNull(builder.Ignore("A", ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.Ignore("A", ConfigurationSource.Convention);
            }

            Assert.NotNull(entityTypeBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPropertyAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalPropertyBuilder propertyBuilder = null;
            var convention = new Mock<IPropertyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    Assert.Equal("OrderId", b.Metadata.Name);
                    propertyBuilder = new InternalPropertyBuilder(b.Metadata, b.ModelBuilder);
                    return propertyBuilder;
                });
            conventions.PropertyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IPropertyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.Same(propertyBuilder, b);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPropertyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.Property("OrderId", typeof(int), ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(entityBuilder.Metadata.AddProperty("OrderId", typeof(int)));
            }

            Assert.NotNull(propertyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPropertyAdded_calls_apply_on_conventions_in_order_for_non_shadow_property(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalPropertyBuilder propertyBuilder = null;
            var convention = new Mock<IPropertyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    Assert.Equal("OrderId", b.Metadata.Name);
                    propertyBuilder = new InternalPropertyBuilder(b.Metadata, b.ModelBuilder);
                    return propertyBuilder;
                });
            conventions.PropertyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IPropertyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.Same(propertyBuilder, b);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPropertyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(entityBuilder.Metadata.AddProperty(Order.OrderIdProperty));
            }

            Assert.NotNull(propertyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPropertyFieldChanged_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalPropertyBuilder expectedPropertyBuilder = null;
            var convention = new Mock<IPropertyFieldChangedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>(), It.IsAny<FieldInfo>()))
                .Returns<InternalPropertyBuilder, FieldInfo>((b, f) =>
            {
                Assert.NotNull(b);
                Assert.Equal("OrderId", b.Metadata.Name);
                Assert.Null(f);
                expectedPropertyBuilder = b;
                return true;
            });
            conventions.PropertyFieldChangedConventions.Add(convention.Object);

            var nullConvention = new Mock<IPropertyFieldChangedConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>(), It.IsAny<FieldInfo>()))
                .Returns<InternalPropertyBuilder, FieldInfo>((b, f) =>
            {
                Assert.Same(expectedPropertyBuilder, b);
                return false;
            });
            conventions.PropertyFieldChangedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPropertyFieldChangedConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>(), It.IsAny<FieldInfo>()))
                .Returns<InternalPropertyBuilder, FieldInfo>((b, f) =>
            {
                Assert.False(true);
                return false;
            });
            conventions.PropertyFieldChangedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            var propertyBuilder = entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention);
            if (useBuilder)
            {
                propertyBuilder.HasField(nameof(Order.IntField), ConfigurationSource.Convention);
            }
            else
            {
                propertyBuilder.Metadata.SetField(nameof(Order.IntField));
            }

            Assert.NotNull(expectedPropertyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnForeignKeyAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var convention = new Mock<IForeignKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.NotNull(b);
                    relationshipBuilder = b;
                    return b;
                });
            conventions.ForeignKeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IForeignKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.Same(relationshipBuilder, b);
                    return null;
                });
            conventions.ForeignKeyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IForeignKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ForeignKeyAddedConventions.Add(extraConvention.Object);

            var modelBuilder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.Relationship(entityBuilder, ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(entityBuilder.Metadata.AddForeignKey(
                    entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata,
                    entityBuilder.Metadata.FindPrimaryKey(),
                    entityBuilder.Metadata,
                    ConfigurationSource.Convention));
            }

            Assert.NotNull(relationshipBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnKeyAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalKeyBuilder keyBuilder = null;
            var convention = new Mock<IKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    keyBuilder = new InternalKeyBuilder(b.Metadata, b.ModelBuilder);
                    return keyBuilder;
                });
            conventions.KeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.Same(keyBuilder, b);
                    return null;
                });
            conventions.KeyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.KeyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.HasKey(new List<string> { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                var property = entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata;
                property.IsNullable = false;
                Assert.Null(entityBuilder.Metadata.AddKey(property));
            }

            Assert.NotNull(keyBuilder);
        }

        [Fact]
        public void OnKeyRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalKeyBuilder keyBuilder = null;
            var convention = new Mock<IKeyRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<Key>()))
                .Callback<InternalEntityTypeBuilder, Key>((b, k) =>
                    {
                        Assert.NotNull(b);
                        Assert.NotNull(k);
                        keyBuilder = new InternalKeyBuilder(k, b.ModelBuilder);
                    });
            conventions.KeyRemovedConventions.Add(convention.Object);

            var extraConvention = new Mock<IKeyRemovedConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<Key>()))
                .Callback<InternalEntityTypeBuilder, Key>((b, k) =>
                    {
                        Assert.NotNull(b);
                        Assert.NotNull(k);
                        Assert.NotNull(keyBuilder);
                    });
            conventions.KeyRemovedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var key = entityBuilder.HasKey(new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            Assert.Same(key, entityBuilder.Metadata.RemoveKey(key.Properties));

            Assert.NotNull(keyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPrimaryKeySet_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalKeyBuilder internalKeyBuilder = null;
            var convention = new Mock<IPrimaryKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Null(t);
                        internalKeyBuilder = b;
                        return true;
                    });
            conventions.PrimaryKeySetConventions.Add(convention.Object);

            var nullConvention = new Mock<IPrimaryKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.Null(t);
                        Assert.Same(internalKeyBuilder, b);
                        return false;
                    });
            conventions.PrimaryKeySetConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPrimaryKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.PrimaryKeySetConventions.Add(extraConvention.Object);

            var entityBuilder = new InternalModelBuilder(new Model(conventions))
                .Entity(typeof(Order), ConfigurationSource.Convention);

            entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(entityBuilder.Metadata.SetPrimaryKey(
                    entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata));
            }

            Assert.NotNull(internalKeyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnIndexAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalIndexBuilder indexBuilder = null;
            var convention = new Mock<IIndexConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.NotNull(b);
                    indexBuilder = new InternalIndexBuilder(b.Metadata, b.ModelBuilder);
                    return indexBuilder;
                });
            conventions.IndexAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IIndexConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.Same(indexBuilder, b);
                    return null;
                });
            conventions.IndexAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IIndexConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.IndexAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                var property = entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata;
                Assert.Null(entityBuilder.Metadata.AddIndex(property));
            }

            Assert.NotNull(indexBuilder);
        }

        [Fact]
        public void OnIndexRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalIndexBuilder keyBuilder = null;
            var convention = new Mock<IIndexRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<Index>()))
                .Callback<InternalEntityTypeBuilder, Index>((b, i) =>
                    {
                        Assert.NotNull(b);
                        Assert.NotNull(i);
                        keyBuilder = new InternalIndexBuilder(i, b.ModelBuilder);
                    });
            conventions.IndexRemovedConventions.Add(convention.Object);

            var extraConvention = new Mock<IIndexRemovedConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<Index>()))
                .Callback<InternalEntityTypeBuilder, Index>((b, k) =>
                    {
                        Assert.NotNull(b);
                        Assert.NotNull(k);
                        Assert.NotNull(keyBuilder);
                    });
            conventions.IndexRemovedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var index = entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            Assert.Same(index, entityBuilder.Metadata.RemoveIndex(index.Properties));

            Assert.NotNull(keyBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnIndexUniquenessChanged_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalIndexBuilder indexBuilder = null;
            var convention = new Mock<IIndexUniquenessConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.NotNull(b);
                    indexBuilder = b;
                    return true;
                });
            conventions.IndexUniquenessConventions.Add(convention.Object);

            var nullConvention = new Mock<IIndexUniquenessConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.Same(indexBuilder, b);
                    return false;
                });
            conventions.IndexUniquenessConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IIndexUniquenessConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalIndexBuilder>())).Returns<InternalIndexBuilder>(b =>
                {
                    Assert.False(true);
                    return false;
                });
            conventions.IndexUniquenessConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            if (useBuilder)
            {
                entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                var property = entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata;
                entityBuilder.Metadata.AddIndex(property).IsUnique = true;
            }

            Assert.NotNull(indexBuilder);
        }

        [Fact]
        public void OnForeignKeyRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var foreignKeyRemoved = false;

            var convention = new Mock<IForeignKeyRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<ForeignKey>()))
                .Callback(() => foreignKeyRemoved = true);
            conventions.ForeignKeyRemovedConventions.Add(convention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = entityBuilder.Metadata.AddForeignKey(
                new[] { entityBuilder.Property("FK", typeof(int), ConfigurationSource.Convention).Metadata },
                entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata);

            Assert.NotNull(entityBuilder.Metadata.RemoveForeignKey(foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));

            Assert.True(foreignKeyRemoved);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnNavigationAdded_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var orderIgnored = false;
            var orderDetailsIgnored = false;
            var convention = new Mock<INavigationConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.NotNull(b);
                    relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder);
                    return relationshipBuilder;
                });
            conventions.NavigationAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<INavigationConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.Same(relationshipBuilder, b);
                    if (n.Name == "Order")
                    {
                        orderIgnored = true;
                    }
                    if (n.Name == "OrderDetails")
                    {
                        orderDetailsIgnored = true;
                    }
                    return null;
                });
            conventions.NavigationAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<INavigationConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.NavigationAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, OrderDetails.OrderProperty, Order.OrderDetailsProperty, ConfigurationSource.Convention));
            }
            else
            {
                var fk = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention)
                    .Metadata;
                Assert.Null(fk.HasDependentToPrincipal(OrderDetails.OrderProperty));
            }

            Assert.True(orderIgnored);
            Assert.False(orderDetailsIgnored);
            Assert.NotNull(relationshipBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnNavigationRemoved_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder dependentEntityTypeBuilderFromConvention = null;
            InternalEntityTypeBuilder principalEntityBuilderFromConvention = null;
            var convention = new Mock<INavigationRemovedConvention>();
            convention.Setup(c => c.Apply(
                It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>(), It.IsAny<PropertyInfo>()))
                .Returns((InternalEntityTypeBuilder s, InternalEntityTypeBuilder t, string n, PropertyInfo p) =>
                    {
                        dependentEntityTypeBuilderFromConvention = s;
                        principalEntityBuilderFromConvention = t;
                        Assert.Equal(nameof(OrderDetails.Order), n);
                        Assert.Equal(nameof(OrderDetails.Order), p.Name);
                        return false;
                    });
            conventions.NavigationRemovedConventions.Add(convention.Object);

            var extraConvention = new Mock<INavigationRemovedConvention>();
            extraConvention.Setup(c => c.Apply(
                It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>(), It.IsAny<PropertyInfo>()))
                .Returns((InternalEntityTypeBuilder s, InternalEntityTypeBuilder t, string n, PropertyInfo p) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.NavigationRemovedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, nameof(OrderDetails.Order), nameof(Order.OrderDetails), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.NotNull(relationshipBuilder.DependentToPrincipal((string)null, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(relationshipBuilder.Metadata.HasDependentToPrincipal((string)null, ConfigurationSource.Convention));
            }

            Assert.Same(dependentEntityBuilder, dependentEntityTypeBuilderFromConvention);
            Assert.Same(principalEntityBuilder, principalEntityBuilderFromConvention);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnForeignKeyUniquenessChanged_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var convention = new Mock<IForeignKeyUniquenessConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
            {
                Assert.NotNull(b);
                relationshipBuilder = b;
                return true;
            });
            conventions.ForeignKeyUniquenessConventions.Add(convention.Object);

            var nullConvention = new Mock<IForeignKeyUniquenessConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
            {
                Assert.Same(relationshipBuilder, b);
                return false;
            });
            conventions.ForeignKeyUniquenessConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IForeignKeyUniquenessConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
            {
                Assert.False(true);
                return false;
            });
            conventions.ForeignKeyUniquenessConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            if (useBuilder)
            {
                dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                 dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention);
            }
            
            Assert.NotNull(relationshipBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPrincipalKeySet_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var convention = new Mock<IPrincipalEndConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.NotNull(b);
                    relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder);
                    return relationshipBuilder;
                });
            conventions.PrincipalEndSetConventions.Add(convention.Object);

            var nullConvention = new Mock<IPrincipalEndConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.Same(relationshipBuilder, b);
                    return null;
                });
            conventions.PrincipalEndSetConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPrincipalEndConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.PrincipalEndSetConventions.Add(extraConvention.Object);

            var modelBuilder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            if (useBuilder)
            {
                Assert.Null(
                    dependentEntityBuilder
                        .Relationship(entityBuilder, ConfigurationSource.Convention)
                        .HasPrincipalKey(entityBuilder.Metadata.FindPrimaryKey().Properties, ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(dependentEntityBuilder.Metadata.AddForeignKey(
                    dependentEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata,
                    entityBuilder.Metadata.FindPrimaryKey(),
                    entityBuilder.Metadata,
                    ConfigurationSource.Convention));
            }

            Assert.NotNull(relationshipBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void InitializingModel_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var nullConventionCalled = false;

            InternalModelBuilder modelBuilder = null;
            var convention = new Mock<IModelConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.NotNull(b);
                    modelBuilder = new InternalModelBuilder(b.Metadata);
                    return b;
                });
            conventions.ModelInitializedConventions.Add(convention.Object);

            var nullConvention = new Mock<IModelConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    nullConventionCalled = true;
                    return null;
                });
            conventions.ModelInitializedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IModelConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ModelInitializedConventions.Add(extraConvention.Object);

            if (useBuilder)
            {
                Assert.NotNull(new ModelBuilder(conventions));
            }
            else
            {
                Assert.NotNull(new Model(conventions));
            }

            Assert.True(nullConventionCalled);
            Assert.NotNull(modelBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void ValidatingModel_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var nullConventionCalled = false;

            InternalModelBuilder modelBuilder = null;
            var convention = new Mock<IModelConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.NotNull(b);
                    modelBuilder = new InternalModelBuilder(b.Metadata);
                    return b;
                });
            conventions.ModelBuiltConventions.Add(convention.Object);

            var nullConvention = new Mock<IModelConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    nullConventionCalled = true;
                    return null;
                });
            conventions.ModelBuiltConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IModelConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ModelBuiltConventions.Add(extraConvention.Object);

            Assert.Null(useBuilder
                ? new InternalModelBuilder(new Model(conventions)).Validate()
                : new Model(conventions).Validate());

            Assert.True(nullConventionCalled);
            Assert.NotNull(modelBuilder);
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPropertyNullableChanged_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyNullableConvention(false);
            var convention2 = new PropertyNullableConvention(true);
            var convention3 = new PropertyNullableConvention(false);

            conventions.PropertyNullableChangedConventions.Add(convention1);
            conventions.PropertyNullableChangedConventions.Add(convention2);
            conventions.PropertyNullableChangedConventions.Add(convention3);

            var builder = new ModelBuilder(conventions);

            var propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired();
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = false;
            }

            Assert.Equal(new bool?[] { false }, convention1.Calls);
            Assert.Equal(new bool?[] { false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired(false);
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = true;
            }

            Assert.Equal(new bool?[] { false, true }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired(false);
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = true;
            }

            Assert.Equal(new bool?[] { false, true }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired();
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = false;
            }

            Assert.Equal(new bool?[] { false, true, false }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class PropertyNullableConvention : IPropertyNullableConvention
        {
            public readonly List<bool?> Calls = new List<bool?>();
            private readonly bool _terminate;

            public PropertyNullableConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalPropertyBuilder propertyBuilder)
            {
                Calls.Add(propertyBuilder.Metadata.IsNullable);

                return !_terminate;
            }
        }

        private class Order
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(Order).GetProperty(nameof(OrderId));
            public static readonly PropertyInfo OrderDetailsProperty = typeof(Order).GetProperty(nameof(OrderDetails));

            public int IntField = 1;

            public int OrderId { get; set; }

            public string Name { get; set; }

            public virtual OrderDetails OrderDetails { get; set; }
        }

        private class SpecialOrder : Order
        {
        }

        private class OrderDetails
        {
            public static readonly PropertyInfo OrderProperty = typeof(OrderDetails).GetProperty(nameof(Order));

            public int Id { get; set; }
            public virtual Order Order { get; set; }
        }
    }
}
