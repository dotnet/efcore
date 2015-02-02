// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetFinderTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            var finder = new DbSetFinder();

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => finder.FindSets(null)).ParamName);
        }

        [Fact]
        public void All_non_static_DbSet_properties_are_discovered()
        {
            using (var context = new The())
            {
                var sets = new DbSetFinder().FindSets(context);

                Assert.Equal(
                    new[] { "Betters", "Brandies", "Drinkings", "Stops", "Yous" },
                    sets.Select(s => s.Name).ToArray());

                Assert.Equal(
                    new[] { typeof(Streets), typeof(The), typeof(The), typeof(Streets), typeof(Streets) },
                    sets.Select(s => s.ContextType).ToArray());

                Assert.Equal(
                    new[] { typeof(Better), typeof(Brandy), typeof(Drinking), typeof(Stop), typeof(You) },
                    sets.Select(s => s.EntityType).ToArray());

                Assert.Equal(
                    new[] { true, true, true, false, true },
                    sets.Select(s => s.HasSetter).ToArray());
            }
        }

        #region Fixture

        public class Streets : DbContext
        {
            public DbSet<You> Yous { get; set; }
            protected DbSet<Better> Betters { get; set; }

            internal DbSet<Stop> Stops
            {
                get { return null; }
            }
        }

        public class The : Streets
        {
            public DbSet<Drinking> Drinkings { get; set; }
            private DbSet<Brandy> Brandies { get; set; }

            public static DbSet<Random> NotMe1 { get; set; }
            public Random NotMe2 { get; set; }
            public List<Random> NotMe3 { get; set; }
            public NotANormalSet<Random> NotMe4 { get; set; }
        }

        public class You
        {
        }

        public class Better
        {
        }

        public class Stop
        {
        }

        public class Drinking
        {
        }

        internal class Brandy
        {
        }

        public class NotANormalSet<TEntity> : DbSet<TEntity>
            where TEntity : class
        {
            public override IReadOnlyList<EntityEntry<TEntity>> Add([NotNull] params TEntity[] entities)
            {
                throw new NotImplementedException();
            }

            public override EntityEntry<TEntity> Add([NotNull] TEntity entity)
            {
                throw new NotImplementedException();
            }

            public override IReadOnlyList<EntityEntry<TEntity>> Attach([NotNull] params TEntity[] entities)
            {
                throw new NotImplementedException();
            }

            public override EntityEntry<TEntity> Attach([NotNull] TEntity entity)
            {
                throw new NotImplementedException();
            }

            public override IReadOnlyList<EntityEntry<TEntity>> Remove([NotNull] params TEntity[] entities)
            {
                throw new NotImplementedException();
            }

            public override EntityEntry<TEntity> Remove([NotNull] TEntity entity)
            {
                throw new NotImplementedException();
            }

            public override IReadOnlyList<EntityEntry<TEntity>> Update([NotNull] params TEntity[] entities)
            {
                throw new NotImplementedException();
            }

            public override EntityEntry<TEntity> Update([NotNull] TEntity entity)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
