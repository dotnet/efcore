// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGenerator.Internal
{
    public class IdValueGeneratorTest
    {
        [ConditionalFact]
        public void Generated_ids_do_not_clash()
        {
            var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Blog>().HasKey(p => new { p.OtherId, p.Id });
            modelBuilder.Entity<Post>().HasKey(p => new { p.OtherId, p.Id });

            modelBuilder.Entity<IntClassEntity>().Property(e => e.Id).HasConversion(IntClass.Converter);
            modelBuilder.Entity<IntStructEntity>().Property(e => e.Id).HasConversion(IntStruct.Converter);
            modelBuilder.Entity<BytesStructEntity>().Property(e => e.Id).HasConversion(BytesStruct.Converter);

            var model = modelBuilder.FinalizeModel();

            var ids = new List<string>
            {
                Create(new Blog { Id = 1, OtherId = 1 }),
                Create(new Post { Id = "1", OtherId = "1" }),
                Create(new Post { Id = "1", OtherId = "1|" }),
                Create(new Post { Id = "|1", OtherId = "1" }),
                Create(new IntClassEntity { Id = new IntClass(1) }),
                Create(new IntClassEntity { Id = new IntClass(2) }),
                Create(new IntStructEntity { Id = new IntStruct(1) }),
                Create(new IntStructEntity { Id = new IntStruct(2) }),
                Create(new BytesStructEntity { Id = new BytesStruct(null) }),
                Create(new BytesStructEntity { Id = new BytesStruct(new byte[0]) }),
                Create(new BytesStructEntity { Id = new BytesStruct(new byte[] { 1 }) }),
                Create(new BytesStructEntity { Id = new BytesStruct(new byte[] { 2, 2 }) }),
            };

            Assert.Equal(ids.Count, new HashSet<string>(ids.Concat(ids)).Count);

            string Create<TEntity>(TEntity entity)
                where TEntity : class, new()
                => (string)CosmosTestHelpers.Instance.CreateInternalEntry(
                    model, EntityState.Added, entity)[model.FindEntityType(typeof(TEntity)).FindProperty("id")];
        }

        private class Blog
        {
            public int Id { get; set; }
            public int OtherId { get; set; }
        }

        private class Post
        {
            public string Id { get; set; }
            public string OtherId { get; set; }
        }

        private class IntClassEntity
        {
            public IntClass Id { get; set; }
        }

        private class IntClass
        {
            public static ValueConverter<IntClass, int> Converter
                = new ValueConverter<IntClass, int>(v => v.Value, v => new IntClass(v));

            public IntClass(int value)
                => Value = value;

            private bool Equals(IntClass other)
                => other != null && Value == other.Value;

            public override bool Equals(object obj)
                => obj == this
                    || obj?.GetType() == GetType()
                    && Equals((IntClass)obj);

            public override int GetHashCode() => Value;

            public int Value { get; }
        }

        private class IntStructEntity
        {
            public IntStruct Id { get; set; }
        }

        private struct IntStruct
        {
            public static ValueConverter<IntStruct, int> Converter
                = new ValueConverter<IntStruct, int>(v => v.Value, v => new IntStruct(v));

            public IntStruct(int value)
                => Value = value;

            public int Value { get; }
        }

        private class BytesStructEntity
        {
            public BytesStruct Id { get; set; }
        }

        private struct BytesStruct
        {
            public static ValueConverter<BytesStruct, byte[]> Converter
                = new ValueConverter<BytesStruct, byte[]>(v => v.Value, v => new BytesStruct(v));

            public BytesStruct(byte[] value)
                => Value = value;

            public byte[] Value { get; }

            public bool Equals(BytesStruct other)
                => (Value == null
                        && other.Value == null)
                    || (other.Value != null
                        && Value?.SequenceEqual(other.Value) == true);

            public override int GetHashCode()
            {
                var code = new HashCode();
                if (Value != null)
                {
                    foreach (var b in Value)
                    {
                        code.Add(b);
                    }
                }

                return code.ToHashCode();
            }
        }
    }
}
