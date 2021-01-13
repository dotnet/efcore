// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMapperTestBase
    {
        protected IMutableEntityType CreateEntityType()
            => CreateModel().FindEntityType(typeof(MyType));

        protected IMutableModel CreateModel()
        {
            var builder = CreateModelBuilder();

            builder.Entity<MyType>().Property(e => e.Id).HasColumnType("money");
            builder.Entity<MyRelatedType1>().Property(e => e.Id).HasMaxLength(200).IsFixedLength();
            builder.Entity<MyRelatedType1>().Property(e => e.Relationship2Id).HasColumnType("dec(6,1)");
            builder.Entity<MyRelatedType2>().Property(e => e.Id).HasMaxLength(100).IsFixedLength();
            builder.Entity<MyRelatedType2>().Property(e => e.Relationship2Id).HasMaxLength(787);
            builder.Entity<MyRelatedType3>().Property(e => e.Id).IsUnicode(false);
            builder.Entity<MyRelatedType3>().Property(e => e.Relationship2Id).HasMaxLength(767);
            builder.Entity<MyRelatedType4>().Property(e => e.Relationship2Id).IsUnicode();
            builder.Entity<MyPrecisionType>().Property(e => e.PrecisionOnly).HasPrecision(16);
            builder.Entity<MyPrecisionType>().Property(e => e.PrecisionAndScale).HasPrecision(18, 7);
            builder.Entity<MyTypeWithIndexAttribute>();

            return builder.Model;
        }

        protected abstract ModelBuilder CreateModelBuilder();

        protected class MyType
        {
            public decimal Id { get; set; }
        }

        protected class MyPrecisionType
        {
            public decimal Id { get; set; }
            public decimal PrecisionOnly { get; set; }
            public decimal PrecisionAndScale { get; set; }
        }

        protected class MyRelatedType1
        {
            public string Id { get; set; }

            public decimal Relationship1Id { get; set; }
            public MyType Relationship1 { get; set; }

            public decimal Relationship2Id { get; set; }
            public MyType Relationship2 { get; set; }
        }

        protected class MyRelatedType2
        {
            public byte[] Id { get; set; }

            public string Relationship1Id { get; set; }
            public MyRelatedType1 Relationship1 { get; set; }

            public string Relationship2Id { get; set; }
            public MyRelatedType1 Relationship2 { get; set; }
        }

        protected class MyRelatedType3
        {
            public string Id { get; set; }

            public byte[] Relationship1Id { get; set; }
            public MyRelatedType2 Relationship1 { get; set; }

            public byte[] Relationship2Id { get; set; }
            public MyRelatedType2 Relationship2 { get; set; }
        }

        protected class MyRelatedType4
        {
            public string Id { get; set; }

            public string Relationship1Id { get; set; }
            public MyRelatedType3 Relationship1 { get; set; }

            public string Relationship2Id { get; set; }
            public MyRelatedType3 Relationship2 { get; set; }
        }

        [Index(nameof(Name))]
        protected class MyTypeWithIndexAttribute
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
