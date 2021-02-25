// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalDatabaseFacadeExtensionsTest
    {
        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_no_params(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync("<Some query>", cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync("<Some query>");
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>");
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(new List<object>(), commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_array_of_int_params_as_object(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync("<Some query>", new object[] { 1, 2 }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync("<Some query>", 1, 2);
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>", 1, 2);
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1, 2 }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Can_pass_ints_as_params(bool async)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>", 1, 2);
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>", 1, 2);
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1, 2 }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_mixed_array_of_params(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync("<Some query>", new object[] { 1, "Cheese" }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync("<Some query>", 1, "Cheese");
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>", 1, "Cheese");
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1, "Cheese" }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_list_of_int_params_as_object(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync(
                        "<Some query>", new List<object> { 1, 2 }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "<Some query>", new List<object> { 1, 2 });
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw(
                    "<Some query>", new List<object> { 1, 2 });
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1, 2 }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_mixed_list_of_params(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync(
                        "<Some query>", new List<object> { 1, "Pickle" }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "<Some query>", new List<object> { 1, "Pickle" });
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw(
                    "<Some query>", new List<object> { 1, "Pickle" });
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1, "Pickle" }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_single_int_as_object(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync("<Some query>", new object[] { 1 }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync("<Some query>", 1);
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>", 1);
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { 1 }, commandBuilder.Parameters);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Can_pass_single_string(bool async, bool cancellation)
        {
            using var context = new ThudContext();
            var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

            if (async)
            {
                if (cancellation)
                {
                    var cancellationToken = new CancellationToken();
                    await context.Database.ExecuteSqlRawAsync("<Some query>", new[] { "Branston" }, cancellationToken);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync("<Some query>", "Branston");
                }
            }
            else
            {
                context.Database.ExecuteSqlRaw("<Some query>", "Branston");
            }

            Assert.Equal("<Some query>", commandBuilder.Sql);
            Assert.Equal(
                new List<object> { "Branston" }, commandBuilder.Parameters);
        }

        private class ThudContext : DbContext
        {
            public ThudContext()
                : base(
                    RelationalTestHelpers.Instance.CreateOptions(
                        RelationalTestHelpers.Instance.CreateServiceProvider(
                            new ServiceCollection()
                                .AddScoped<IRawSqlCommandBuilder, TestRawSqlCommandBuilder>())))
            {
            }
        }

        private class TestRawSqlCommandBuilder : IRawSqlCommandBuilder
        {
            private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

            public TestRawSqlCommandBuilder(
                IRelationalCommandBuilderFactory relationalCommandBuilderFactory)
            {
                _commandBuilderFactory = relationalCommandBuilderFactory;
            }

            public string Sql { get; private set; }
            public IEnumerable<object> Parameters { get; private set; }

            public IRelationalCommand Build(string sql)
                => throw new NotImplementedException();

            public RawSqlCommand Build(string sql, IEnumerable<object> parameters)
            {
                Sql = sql;
                Parameters = parameters;

                return new RawSqlCommand(_commandBuilderFactory.Create().Build(), new Dictionary<string, object>());
            }
        }
    }
}
