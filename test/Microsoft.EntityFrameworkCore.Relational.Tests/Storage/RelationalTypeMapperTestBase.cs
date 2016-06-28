// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Storage
{
    public class RelationalTypeMapperTestBase
    {
        protected static EntityType CreateEntityType()
            => (EntityType)CreateModel().FindEntityType(typeof(MyType));

        protected static IModel CreateModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<MyType>().Property(e => e.Id).HasColumnType("money");
            builder.Entity<MyRelatedType1>().Property(e => e.Id).HasMaxLength(200);
            builder.Entity<MyRelatedType1>().Property(e => e.Relationship2Id).HasColumnType("dec");
            builder.Entity<MyRelatedType2>().Property(e => e.Id).HasMaxLength(100);
            builder.Entity<MyRelatedType2>().Property(e => e.Relationship2Id).HasMaxLength(787);
            builder.Entity<MyRelatedType3>().Property(e => e.Id).IsUnicode(false);
            builder.Entity<MyRelatedType3>().Property(e => e.Relationship2Id).HasMaxLength(767);
            builder.Entity<MyRelatedType4>().Property(e => e.Relationship2Id).IsUnicode();

            return builder.Model;
        }

        protected class MyType
        {
            public decimal Id { get; set; }
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
    }
}
