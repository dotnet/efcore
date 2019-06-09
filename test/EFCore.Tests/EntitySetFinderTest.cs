// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class DbSetFinderTest
    {
        [ConditionalFact]
        public void All_non_static_DbSet_properties_are_discovered()
        {
            using (var context = new The())
            {
                var sets = new DbSetFinder().FindSets(context.GetType());

                Assert.Equal(
                    new[] { "Betters", "Brandies", "Drinkings", "Stops", "Yous" },
                    sets.Select(s => s.Name).ToArray());

                Assert.Equal(
                    new[] { typeof(Better), typeof(Brandy), typeof(Drinking), typeof(Stop), typeof(You) },
                    sets.Select(s => s.ClrType).ToArray());

                Assert.Equal(
                    new[] { true, true, true, false, true },
                    sets.Select(s => s.Setter != null).ToArray());
            }
        }

        #region Fixture

        public class Streets : DbContext
        {
            public DbSet<You> Yous { get; set; }
            protected DbSet<Better> Betters { get; set; }

            internal DbSet<Stop> Stops => null;
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
        }

        #endregion
    }
}
