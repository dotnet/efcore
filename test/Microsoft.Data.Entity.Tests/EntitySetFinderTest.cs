// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntitySetFinderTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            var finder = new EntitySetFinder();

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => finder.FindSets(null)).ParamName);
        }

        [Fact]
        public void All_non_static_EntitySet_properties_are_discovered()
        {
            using (var context = new The())
            {
                var sets = new EntitySetFinder().FindSets(context);

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

        public class Streets : EntityContext
        {
            public Streets()
                : base(new EntityConfiguration())
            {
            }

            public EntitySet<You> Yous { get; set; }
            protected EntitySet<Better> Betters { get; set; }

            internal EntitySet<Stop> Stops
            {
                get { return null; }
            }
        }

        public class The : Streets
        {
            public EntitySet<Drinking> Drinkings { get; set; }
            private EntitySet<Brandy> Brandies { get; set; }

            public static EntitySet<Random> NotMe1 { get; set; }
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

        public class NotANormalSet<TEntity> : EntitySet<TEntity>
            where TEntity : class
        {
        }

        #endregion
    }
}
