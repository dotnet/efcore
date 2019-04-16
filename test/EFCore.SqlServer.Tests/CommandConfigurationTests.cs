// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore
{
    public class CommandConfigurationTests
    {
        public class CommandTimeout
        {
            [Fact]
            public void Default_value_for_CommandTimeout_is_null_and_can_be_changed_including_setting_to_null()
            {
                using (var context = new TimeoutContext())
                {
                    Assert.Null(context.Database.GetCommandTimeout());

                    context.Database.SetCommandTimeout(77);
                    Assert.Equal(77, context.Database.GetCommandTimeout());

                    context.Database.SetCommandTimeout(null);
                    Assert.Null(context.Database.GetCommandTimeout());

                    context.Database.SetCommandTimeout(TimeSpan.FromSeconds(66));
                    Assert.Equal(66, context.Database.GetCommandTimeout());
                }
            }

            [Fact]
            public void Setting_CommandTimeout_to_negative_value_throws()
            {
                Assert.Throws<InvalidOperationException>(
                    () => new DbContextOptionsBuilder().UseSqlServer(
                        "No=LoveyDovey",
                        b => b.CommandTimeout(-55)));

                using (var context = new TimeoutContext())
                {
                    Assert.Null(context.Database.GetCommandTimeout());

                    Assert.Throws<ArgumentException>(
                        () => context.Database.SetCommandTimeout(-3));
                    Assert.Throws<ArgumentException>(
                        () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(-3)));

                    Assert.Throws<ArgumentException>(
                        () => context.Database.SetCommandTimeout(-99));
                    Assert.Throws<ArgumentException>(
                        () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(-99)));

                    Assert.Throws<ArgumentException>(
                        () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(uint.MaxValue)));
                }
            }

            public class TimeoutContext : DbContext
            {
                public TimeoutContext()
                {
                }

                public TimeoutContext(int? commandTimeout)
                    => Database.SetCommandTimeout(commandTimeout);

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.UseSqlServer(new FakeDbConnection("A=B"));
            }
        }
    }
}
