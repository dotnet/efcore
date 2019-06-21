// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class InterceptionTestBase<TBuilder, TExtension>
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension, new()
    {
        protected InterceptionTestBase(InterceptionFixtureBase fixture)
        {
            Fixture = fixture;
        }

        protected InterceptionFixtureBase Fixture { get; }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task<string> Intercept_query_passively(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<PassiveReaderCommandInterceptor>(inject);
            using (context)
            {
                List<Singularity> results;
                if (async)
                {
                    results = await context.Set<Singularity>().ToListAsync();
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    results = context.Set<Singularity>().ToList();
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole", results[0].Type);
                Assert.Equal("Bing Bang", results[1].Type);

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);
            }

            return interceptor.CommandText;
        }

        protected class PassiveReaderCommandInterceptor : CommandInterceptorBase
        {
            public PassiveReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_passively(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<PassiveScalarCommandInterceptor>(inject);
            using (context)
            {
                var sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                if (async)
                {
                    Assert.Equal(
                        1, Convert.ToInt32(
                            await command.ExecuteScalarAsync(
                                new RelationalCommandParameterObject(
                                    connection, null, context, logger))));
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    Assert.Equal(
                        1, Convert.ToInt32(
                            command.ExecuteScalar(
                                new RelationalCommandParameterObject(connection, null, context, logger))));
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);

                AssertSql(sql, interceptor.CommandText);
            }
        }

        protected class PassiveScalarCommandInterceptor : CommandInterceptorBase
        {
            public PassiveScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_passively(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<PassiveNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 77";

                    if (async)
                    {
                        Assert.Equal(1, await context.Database.ExecuteSqlRawAsync(nonQuery));
                        Assert.True(interceptor.AsyncCalled);
                    }
                    else
                    {
                        Assert.Equal(1, context.Database.ExecuteSqlRaw(nonQuery));
                        Assert.True(interceptor.SyncCalled);
                    }

                    Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                    Assert.True(interceptor.ExecutingCalled);
                    Assert.True(interceptor.ExecutedCalled);
                    Assert.Same(context, interceptor.Context);

                    AssertSql(nonQuery, interceptor.CommandText);
                }
            }
        }

        protected class PassiveNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public PassiveNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task<string> Intercept_query_to_suppress_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<SuppressingReaderCommandInterceptor>(inject);
            using (context)
            {
                List<Singularity> results;

                if (async)
                {
                    results = await context.Set<Singularity>().ToListAsync();
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    results = context.Set<Singularity>().ToList();
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.Equal(3, results.Count);
                Assert.Equal(977, results[0].Id);
                Assert.Equal(988, results[1].Id);
                Assert.Equal(999, results[2].Id);
                Assert.Equal("<977>", results[0].Type);
                Assert.Equal("<988>", results[1].Type);
                Assert.Equal("<999>", results[2].Type);

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);
            }

            return interceptor.CommandText;
        }

        protected class SuppressingReaderCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader>? ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result)
            {
                base.ReaderExecuting(command, eventData, result);

                return new InterceptionResult<DbDataReader>(new FakeDbDataReader());
            }

            public override Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result,
                CancellationToken cancellationToken = default)
            {
                base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

                return Task.FromResult<InterceptionResult<DbDataReader>?>(new InterceptionResult<DbDataReader>(new FakeDbDataReader()));
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_to_suppress_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<SuppressingScalarCommandInterceptor>(inject);
            using (context)
            {
                var sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                if (async)
                {
                    Assert.Equal(
                        SuppressingScalarCommandInterceptor.InterceptedResult,
                        await command.ExecuteScalarAsync(
                            new RelationalCommandParameterObject(
                                connection, null, context, logger)));

                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    Assert.Equal(
                        SuppressingScalarCommandInterceptor.InterceptedResult,
                        command.ExecuteScalar(
                            new RelationalCommandParameterObject(
                                connection, null, context, logger)));

                    Assert.True(interceptor.SyncCalled);
                }

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);

                AssertSql(sql, interceptor.CommandText);
            }
        }

        protected class SuppressingScalarCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public const string InterceptedResult = "Bet you weren't expecting a string!";

            public override InterceptionResult<object>? ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result)
            {
                base.ScalarExecuting(command, eventData, result);

                return new InterceptionResult<object>(InterceptedResult);
            }

            public override Task<InterceptionResult<object>?> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result,
                CancellationToken cancellationToken = default)
            {
                base.ScalarExecutingAsync(command, eventData, result, cancellationToken);

                return Task.FromResult<InterceptionResult<object>?>(
                    new InterceptionResult<object>(InterceptedResult));
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_suppress_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<SuppressingNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 77";

                    if (async)
                    {
                        Assert.Equal(2, await context.Database.ExecuteSqlRawAsync(nonQuery));
                        Assert.True(interceptor.AsyncCalled);
                    }
                    else
                    {
                        Assert.Equal(2, context.Database.ExecuteSqlRaw(nonQuery));
                        Assert.True(interceptor.SyncCalled);
                    }

                    Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                    Assert.True(interceptor.ExecutingCalled);
                    Assert.True(interceptor.ExecutedCalled);
                    Assert.Same(context, interceptor.Context);

                    AssertSql(nonQuery, interceptor.CommandText);
                }
            }
        }

        protected class SuppressingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }

            public override InterceptionResult<int>? NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result)
            {
                base.NonQueryExecuting(command, eventData, result);

                return new InterceptionResult<int>(2);
            }

            public override Task<InterceptionResult<int>?> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result,
                CancellationToken cancellationToken = default)
            {
                base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

                return Task.FromResult<InterceptionResult<int>?>(new InterceptionResult<int>(2));
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task<string> Intercept_query_to_mutate_command(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<MutatingReaderCommandInterceptor>(inject);
            using (context)
            {
                List<Singularity> results;

                if (async)
                {
                    results = await context.Set<Singularity>().ToListAsync();
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    results = context.Set<Singularity>().ToList();
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole?", results[0].Type);
                Assert.Equal("Bing Bang?", results[1].Type);

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);
            }

            return interceptor.CommandText;
        }

        protected class MutatingReaderCommandInterceptor : CommandInterceptorBase
        {
            public MutatingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader>? ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result)
            {
                MutateQuery(command);

                return base.ReaderExecuting(command, eventData, result);
            }

            public override Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result,
                CancellationToken cancellationToken = default)
            {
                MutateQuery(command);

                return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
            }

            private static void MutateQuery(DbCommand command)
                => command.CommandText = command.CommandText.Replace("Singularity", "Brane");
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_to_mutate_command(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<MutatingScalarCommandInterceptor>(inject);
            using (context)
            {
                var sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                if (async)
                {
                    Assert.Equal(2, Convert.ToInt32(await command.ExecuteScalarAsync(
                        new RelationalCommandParameterObject(connection, null, context, logger))));
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    Assert.Equal(
                        2, Convert.ToInt32(
                            command.ExecuteScalar(
                                new RelationalCommandParameterObject(
                                    connection, null, context, logger))));
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);

                AssertSql(MutatingScalarCommandInterceptor.MutatedSql, interceptor.CommandText);
            }
        }

        protected class MutatingScalarCommandInterceptor : CommandInterceptorBase
        {
            public MutatingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public const string MutatedSql = "SELECT 2";

            public override InterceptionResult<object>? ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result)
            {
                command.CommandText = MutatedSql;

                return base.ScalarExecuting(command, eventData, result);
            }

            public override Task<InterceptionResult<object>?> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result,
                CancellationToken cancellationToken = default)
            {
                command.CommandText = MutatedSql;

                return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_mutate_command(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<MutatingNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 77";

                    if (async)
                    {
                        Assert.Equal(0, await context.Database.ExecuteSqlRawAsync(nonQuery));
                        Assert.True(interceptor.AsyncCalled);
                    }
                    else
                    {
                        Assert.Equal(0, context.Database.ExecuteSqlRaw(nonQuery));
                        Assert.True(interceptor.SyncCalled);
                    }

                    Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                    Assert.True(interceptor.ExecutingCalled);
                    Assert.True(interceptor.ExecutedCalled);
                    Assert.Same(context, interceptor.Context);

                    AssertSql(MutatingNonQueryCommandInterceptor.MutatedSql, interceptor.CommandText);
                }
            }
        }

        protected class MutatingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public const string MutatedSql = "DELETE FROM Singularity WHERE Id = 78";

            public MutatingNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }

            public override InterceptionResult<int>? NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result)
            {
                command.CommandText = MutatedSql;

                return base.NonQueryExecuting(command, eventData, result);
            }

            public override Task<InterceptionResult<int>?> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result,
                CancellationToken cancellationToken = default)
            {
                command.CommandText = MutatedSql;

                return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task<string> Intercept_query_to_replace_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<QueryReplacingReaderCommandInterceptor>(inject);
            using (context)
            {
                List<Singularity> results;

                if (async)
                {
                    results = await context.Set<Singularity>().ToListAsync();
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    results = context.Set<Singularity>().ToList();
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole?", results[0].Type);
                Assert.Equal("Bing Bang?", results[1].Type);

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);
            }

            return interceptor.CommandText;
        }

        protected class QueryReplacingReaderCommandInterceptor : CommandInterceptorBase
        {
            public QueryReplacingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader>? ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result)
            {
                base.ReaderExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<DbDataReader>(CreateNewCommand(command).ExecuteReader());
            }

            public override async Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result,
                CancellationToken cancellationToken = default)
            {
                await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<DbDataReader>(await CreateNewCommand(command).ExecuteReaderAsync(cancellationToken));
            }

            private static DbCommand CreateNewCommand(DbCommand command)
            {
                var newCommand = command.Connection.CreateCommand();
                newCommand.CommandText = command.CommandText.Replace("Singularity", "Brane");

                return newCommand;
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_to_replace_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<QueryReplacingScalarCommandInterceptor>(inject);
            using (context)
            {
                var sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                if (async)
                {
                    Assert.Equal(2, Convert.ToInt32(await command.ExecuteScalarAsync(
                        new RelationalCommandParameterObject(connection, null, context, logger))));
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    Assert.Equal(
                        2, Convert.ToInt32(
                            command.ExecuteScalar(
                                new RelationalCommandParameterObject(connection, null, context, logger))));
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);

                AssertSql(sql, interceptor.CommandText);
            }
        }

        protected class QueryReplacingScalarCommandInterceptor : CommandInterceptorBase
        {
            public QueryReplacingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public override InterceptionResult<object>? ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result)
            {
                base.ScalarExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<object>(CreateNewCommand(command).ExecuteScalar());
            }

            public override async Task<InterceptionResult<object>?> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result,
                CancellationToken cancellationToken = default)
            {
                await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<object>(await CreateNewCommand(command).ExecuteScalarAsync(cancellationToken));
            }

            private static DbCommand CreateNewCommand(DbCommand command)
            {
                var newCommand = command.Connection.CreateCommand();
                newCommand.CommandText = "SELECT 2";

                return newCommand;
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_replace_execution(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<QueryReplacingNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 78";

                    if (async)
                    {
                        Assert.Equal(1, await context.Database.ExecuteSqlRawAsync(nonQuery));
                        Assert.True(interceptor.AsyncCalled);
                    }
                    else
                    {
                        Assert.Equal(1, context.Database.ExecuteSqlRaw(nonQuery));
                        Assert.True(interceptor.SyncCalled);
                    }

                    Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                    Assert.True(interceptor.ExecutingCalled);
                    Assert.True(interceptor.ExecutedCalled);
                    Assert.Same(context, interceptor.Context);

                    AssertSql(nonQuery, interceptor.CommandText);
                }
            }
        }

        protected class QueryReplacingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public QueryReplacingNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }

            public override InterceptionResult<int>? NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result)
            {
                base.NonQueryExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<int>(CreateNewCommand(command).ExecuteNonQuery());
            }

            public override async Task<InterceptionResult<int>?> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result,
                CancellationToken cancellationToken = default)
            {
                await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return new InterceptionResult<int>(await CreateNewCommand(command).ExecuteNonQueryAsync(cancellationToken));
            }

            private DbCommand CreateNewCommand(DbCommand command)
            {
                var newCommand = command.Connection.CreateCommand();
                newCommand.Transaction = command.Transaction;
                newCommand.CommandText = "DELETE FROM Singularity WHERE Id = 77";

                return newCommand;
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task<string> Intercept_query_to_replace_result(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<ResultReplacingReaderCommandInterceptor>(inject);
            using (context)
            {
                List<Singularity> results;

                if (async)
                {
                    results = await context.Set<Singularity>().ToListAsync();
                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    results = context.Set<Singularity>().ToList();
                    Assert.True(interceptor.SyncCalled);
                }

                Assert.Equal(5, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal(977, results[2].Id);
                Assert.Equal(988, results[3].Id);
                Assert.Equal(999, results[4].Id);
                Assert.Equal("Black Hole", results[0].Type);
                Assert.Equal("Bing Bang", results[1].Type);
                Assert.Equal("<977>", results[2].Type);
                Assert.Equal("<988>", results[3].Type);
                Assert.Equal("<999>", results[4].Type);

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);
            }

            return interceptor.CommandText;
        }

        protected class ResultReplacingReaderCommandInterceptor : CommandInterceptorBase
        {
            public ResultReplacingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override DbDataReader ReaderExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result)
            {
                base.ReaderExecuted(command, eventData, result);

                return new CompositeFakeDbDataReader(result, new FakeDbDataReader());
            }

            public override Task<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                base.ReaderExecutedAsync(command, eventData, result, cancellationToken);

                return Task.FromResult<DbDataReader>(new CompositeFakeDbDataReader(result, new FakeDbDataReader()));
            }
        }

        private class CompositeFakeDbDataReader : FakeDbDataReader
        {
            private readonly DbDataReader _firstReader;
            private readonly DbDataReader _secondReader;
            private bool _movedToSecond;

            public CompositeFakeDbDataReader(DbDataReader firstReader, DbDataReader secondReader)
            {
                _firstReader = firstReader;
                _secondReader = secondReader;
            }

            public override void Close()
            {
                _firstReader.Close();
                _secondReader.Close();
            }

            protected override void Dispose(bool disposing)
            {
                _firstReader.Dispose();
                _secondReader.Dispose();

                base.Dispose(disposing);
            }

            public override bool Read()
            {
                if (_movedToSecond)
                {
                    return _secondReader.Read();
                }

                if (!_firstReader.Read())
                {
                    _movedToSecond = true;
                    return _secondReader.Read();
                }

                return true;
            }

            public override int GetInt32(int ordinal)
                => _movedToSecond
                    ? _secondReader.GetInt32(ordinal)
                    : _firstReader.GetInt32(ordinal);

            public override string GetString(int ordinal)
                => _movedToSecond
                    ? _secondReader.GetString(ordinal)
                    : _firstReader.GetString(ordinal);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_to_replace_result(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<ResultReplacingScalarCommandInterceptor>(inject);
            using (context)
            {
                var sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                if (async)
                {
                    Assert.Equal(
                        ResultReplacingScalarCommandInterceptor.InterceptedResult,
                        await command.ExecuteScalarAsync(
                            new RelationalCommandParameterObject(connection, null, context, logger)));

                    Assert.True(interceptor.AsyncCalled);
                }
                else
                {
                    Assert.Equal(
                        ResultReplacingScalarCommandInterceptor.InterceptedResult,
                        command.ExecuteScalar(
                            new RelationalCommandParameterObject(connection, null, context, logger)));

                    Assert.True(interceptor.SyncCalled);
                }

                Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                Assert.True(interceptor.ExecutingCalled);
                Assert.True(interceptor.ExecutedCalled);
                Assert.Same(context, interceptor.Context);

                AssertSql(sql, interceptor.CommandText);
            }
        }

        protected class ResultReplacingScalarCommandInterceptor : CommandInterceptorBase
        {
            public const string InterceptedResult = "Bet you weren't expecting a string!";

            public ResultReplacingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public override object ScalarExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result)
            {
                base.ScalarExecuted(command, eventData, result);

                return InterceptedResult;
            }

            public override Task<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result,
                CancellationToken cancellationToken = default)
            {
                base.ScalarExecutedAsync(command, eventData, result, cancellationToken);

                return Task.FromResult<object>(InterceptedResult);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_replaceresult(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<ResultReplacingNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 78";

                    if (async)
                    {
                        Assert.Equal(7, await context.Database.ExecuteSqlRawAsync(nonQuery));
                        Assert.True(interceptor.AsyncCalled);
                    }
                    else
                    {
                        Assert.Equal(7, context.Database.ExecuteSqlRaw(nonQuery));
                        Assert.True(interceptor.SyncCalled);
                    }

                    Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
                    Assert.True(interceptor.ExecutingCalled);
                    Assert.True(interceptor.ExecutedCalled);
                    Assert.Same(context, interceptor.Context);

                    AssertSql(nonQuery, interceptor.CommandText);
                }
            }
        }

        protected class ResultReplacingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public ResultReplacingNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }

            public override int NonQueryExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result)
            {
                base.NonQueryExecuted(command, eventData, result);

                return 7;
            }

            public override async Task<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);

                return 7;
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_query_to_throw(bool async, bool inject)
        {
            using (var context = CreateContext(new ThrowingReaderCommandInterceptor()))
            {
                var exception = async
                    ? await Assert.ThrowsAsync<Exception>(() => context.Set<Singularity>().ToListAsync())
                    : Assert.Throws<Exception>(() => context.Set<Singularity>().ToList());

                Assert.Equal("Bang!", exception.Message);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_to_throw(bool async, bool inject)
        {
            using (var context = CreateContext(new ThrowingReaderCommandInterceptor()))
            {
                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append("SELECT 1").Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

                var exception = async
                    ? await Assert.ThrowsAsync<Exception>(() => command.ExecuteScalarAsync(
                        new RelationalCommandParameterObject(connection, null, context, logger)))
                    : Assert.Throws<Exception>(() => command.ExecuteScalar(
                        new RelationalCommandParameterObject(connection, null, context, logger)));

                Assert.Equal("Bang!", exception.Message);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_throw(bool async, bool inject)
        {
            using (var context = CreateContext(new ThrowingReaderCommandInterceptor()))
            {
                using (context.Database.BeginTransaction())
                {
                    var nonQuery = "DELETE FROM Singularity WHERE Id = 77";

                    var exception = async
                        ? await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteSqlRawAsync(nonQuery))
                        : Assert.Throws<Exception>(() => context.Database.ExecuteSqlRaw(nonQuery));

                    Assert.Equal("Bang!", exception.Message);
                }
            }
        }

        protected class ThrowingReaderCommandInterceptor : DbCommandInterceptor
        {
            public override InterceptionResult<DbDataReader>? ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result)
            {
                throw new Exception("Bang!");
            }

            public override Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result,
                CancellationToken cancellationToken = default)
            {
                throw new Exception("Bang!");
            }

            public override InterceptionResult<object>? ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result)
            {
                throw new Exception("Bang!");
            }

            public override Task<InterceptionResult<object>?> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result,
                CancellationToken cancellationToken = default)
            {
                throw new Exception("Bang!");
            }

            public override InterceptionResult<int>? NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result)
            {
                throw new Exception("Bang!");
            }

            public override Task<InterceptionResult<int>?> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result,
                CancellationToken cancellationToken = default)
            {
                throw new Exception("Bang!");
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_query_with_one_app_and_one_injected_interceptor(bool async)
        {
            var appInterceptor = new ResultReplacingReaderCommandInterceptor();
            using (var context = CreateContext(appInterceptor, typeof(MutatingReaderCommandInterceptor)))
            {
                await TestCompoisteQueryInterceptors(
                    context,
                    appInterceptor,
                    (MutatingReaderCommandInterceptor)context.GetService<IEnumerable<IDbCommandInterceptor>>().Single(),
                    async);
            }
        }

        private static async Task TestCompoisteQueryInterceptors(
            UniverseContext context,
            ResultReplacingReaderCommandInterceptor interceptor1,
            MutatingReaderCommandInterceptor interceptor2,
            bool async)
        {
            List<Singularity> results;

            if (async)
            {
                results = await context.Set<Singularity>().ToListAsync();
                Assert.True(interceptor1.AsyncCalled);
                Assert.True(interceptor2.AsyncCalled);
            }
            else
            {
                results = context.Set<Singularity>().ToList();
                Assert.True(interceptor1.SyncCalled);
                Assert.True(interceptor2.SyncCalled);
            }

            AssertCompositeResults(results);

            Assert.NotEqual(interceptor1.AsyncCalled, interceptor1.SyncCalled);
            Assert.NotEqual(interceptor2.AsyncCalled, interceptor2.SyncCalled);
            Assert.True(interceptor1.ExecutingCalled);
            Assert.True(interceptor2.ExecutingCalled);
            Assert.True(interceptor1.ExecutedCalled);
            Assert.True(interceptor2.ExecutedCalled);
            Assert.Same(context, interceptor1.Context);
            Assert.Same(context, interceptor2.Context);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_scalar_with_one_app_and_one_injected_interceptor(bool async)
        {
            using (var context = CreateContext(
                new ResultReplacingScalarCommandInterceptor(),
                typeof(MutatingScalarCommandInterceptor)))
            {
                await TestCompositeScalarInterceptors(context, async);
            }
        }

        private static async Task TestCompositeScalarInterceptors(UniverseContext context, bool async)
        {
            var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append("SELECT 1").Build();
            var connection = context.GetService<IRelationalConnection>();
            var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>();

            Assert.Equal(
                ResultReplacingScalarCommandInterceptor.InterceptedResult,
                async
                    ? await command.ExecuteScalarAsync(
                        new RelationalCommandParameterObject(connection, null, context, logger))
                    : command.ExecuteScalar(
                        new RelationalCommandParameterObject(connection, null, context, logger)));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_one_app_and_one_injected_interceptor(bool async)
        {
            using (var context = CreateContext(
                new ResultReplacingNonQueryCommandInterceptor(),
                typeof(MutatingNonQueryCommandInterceptor)))
            {
                await TestCompositeNonQueryInterceptors(context, async);
            }
        }

        private static async Task TestCompositeNonQueryInterceptors(UniverseContext context, bool async)
        {
            using (context.Database.BeginTransaction())
            {
                var nonQuery = "DELETE FROM Singularity WHERE Id = 78";

                Assert.Equal(
                    7,
                    async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_query_with_two_injected_interceptors(bool async)
        {
            using (var context = CreateContext(
                null,
                typeof(MutatingReaderCommandInterceptor),
                typeof(ResultReplacingReaderCommandInterceptor)))
            {
                var injectedInterceptors = context.GetService<IEnumerable<IDbCommandInterceptor>>().ToList();

                await TestCompoisteQueryInterceptors(
                    context,
                    injectedInterceptors.OfType<ResultReplacingReaderCommandInterceptor>().Single(),
                    injectedInterceptors.OfType<MutatingReaderCommandInterceptor>().Single(),
                    async);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_scalar_with_two_injected_interceptors(bool async)
        {
            using (var context = CreateContext(
                null,
                typeof(MutatingScalarCommandInterceptor),
                typeof(ResultReplacingScalarCommandInterceptor)))
            {
                await TestCompositeScalarInterceptors(context, async);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_with_two_injected_interceptors(bool async)
        {
            using (var context = CreateContext(
                null,
                typeof(MutatingNonQueryCommandInterceptor),
                typeof(ResultReplacingNonQueryCommandInterceptor)))
            {
                await TestCompositeNonQueryInterceptors(context, async);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_query_with_explicitly_composed_app_interceptor(bool async)
        {
            using (var context = CreateContext(
                new CompositeDbCommandInterceptor(
                    new MutatingReaderCommandInterceptor(), new ResultReplacingReaderCommandInterceptor())))
            {
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                AssertCompositeResults(results);
            }
        }

        private static void AssertCompositeResults(List<Singularity> results)
        {
            Assert.Equal(5, results.Count);
            Assert.Equal(77, results[0].Id);
            Assert.Equal(88, results[1].Id);
            Assert.Equal(977, results[2].Id);
            Assert.Equal(988, results[3].Id);
            Assert.Equal(999, results[4].Id);
            Assert.Equal("Black Hole?", results[0].Type);
            Assert.Equal("Bing Bang?", results[1].Type);
            Assert.Equal("<977>", results[2].Type);
            Assert.Equal("<988>", results[3].Type);
            Assert.Equal("<999>", results[4].Type);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_scalar_with_explicitly_composed_app_interceptor(bool async)
        {
            using (var context = CreateContext(
                new CompositeDbCommandInterceptor(
                    new MutatingScalarCommandInterceptor(), new ResultReplacingScalarCommandInterceptor())))
            {
                await TestCompositeScalarInterceptors(context, async);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_with_explicitly_composed_app_interceptor(bool async)
        {
            using (var context = CreateContext(
                new CompositeDbCommandInterceptor(
                    new MutatingNonQueryCommandInterceptor(), new ResultReplacingNonQueryCommandInterceptor())))
            {
                await TestCompositeNonQueryInterceptors(context, async);
            }
        }

        private class FakeDbDataReader : DbDataReader
        {
            private int _index;

            private readonly int[] _ints =
            {
                977, 988, 999
            };

            private readonly string[] _strings =
            {
                "<977>", "<988>", "<999>"
            };

            public override bool Read()
                => _index++ < _ints.Length;

            public override int GetInt32(int ordinal)
                => _ints[_index - 1];

            public override bool IsDBNull(int ordinal)
                => false;

            public override string GetString(int ordinal)
                => _strings[_index - 1];

            public override bool GetBoolean(int ordinal) => throw new NotImplementedException();
            public override byte GetByte(int ordinal) => throw new NotImplementedException();

            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                => throw new NotImplementedException();

            public override char GetChar(int ordinal) => throw new NotImplementedException();

            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                => throw new NotImplementedException();

            public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();
            public override DateTime GetDateTime(int ordinal) => throw new NotImplementedException();
            public override decimal GetDecimal(int ordinal) => throw new NotImplementedException();
            public override double GetDouble(int ordinal) => throw new NotImplementedException();
            public override Type GetFieldType(int ordinal) => throw new NotImplementedException();
            public override float GetFloat(int ordinal) => throw new NotImplementedException();
            public override Guid GetGuid(int ordinal) => throw new NotImplementedException();
            public override short GetInt16(int ordinal) => throw new NotImplementedException();
            public override long GetInt64(int ordinal) => throw new NotImplementedException();
            public override string GetName(int ordinal) => throw new NotImplementedException();
            public override int GetOrdinal(string name) => throw new NotImplementedException();
            public override object GetValue(int ordinal) => throw new NotImplementedException();
            public override int GetValues(object[] values) => throw new NotImplementedException();
            public override int FieldCount { get; }
            public override object this[int ordinal] => throw new NotImplementedException();
            public override object this[string name] => throw new NotImplementedException();
            public override int RecordsAffected { get; }
            public override bool HasRows { get; }
            public override bool IsClosed { get; }
            public override bool NextResult() => throw new NotImplementedException();
            public override int Depth { get; }
            public override IEnumerator GetEnumerator() => throw new NotImplementedException();
        }

        protected abstract class CommandInterceptorBase : IDbCommandInterceptor
        {
            private readonly DbCommandMethod _commandMethod;

            protected CommandInterceptorBase(DbCommandMethod commandMethod)
            {
                _commandMethod = commandMethod;
            }

            public DbContext Context { get; set; }
            public string CommandText { get; set; }
            public Guid CommandId { get; set; }
            public Guid ConnectionId { get; set; }
            public bool AsyncCalled { get; set; }
            public bool SyncCalled { get; set; }
            public bool ExecutingCalled { get; set; }
            public bool ExecutedCalled { get; set; }

            public virtual InterceptionResult<DbDataReader>? ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual InterceptionResult<object>? ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual InterceptionResult<int>? NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader>? result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return Task.FromResult(result);
            }

            public virtual Task<InterceptionResult<object>?> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object>? result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return Task.FromResult(result);
            }

            public virtual Task<InterceptionResult<int>?> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int>? result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return Task.FromResult(result);
            }

            public virtual DbDataReader ReaderExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuted(command, eventData);

                return result;
            }

            public virtual object ScalarExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuted(command, eventData);

                return result;
            }

            public virtual int NonQueryExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuted(command, eventData);

                return result;
            }

            public virtual Task<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return Task.FromResult(result);
            }

            public virtual Task<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return Task.FromResult(result);
            }

            public virtual Task<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return Task.FromResult(result);
            }

            protected virtual void AssertExecuting(DbCommand command, CommandEventData eventData)
            {
                Assert.NotNull(eventData.Context);
                Assert.NotEqual(default, eventData.CommandId);
                Assert.NotEqual(default, eventData.ConnectionId);
                Assert.Equal(_commandMethod, eventData.ExecuteMethod);

                Context = eventData.Context;
                CommandText = command.CommandText;
                CommandId = eventData.CommandId;
                ConnectionId = eventData.ConnectionId;
                ExecutingCalled = true;
            }

            protected virtual void AssertExecuted(DbCommand command, CommandExecutedEventData eventData)
            {
                Assert.Same(Context, eventData.Context);
                Assert.Equal(CommandText, command.CommandText);
                Assert.Equal(CommandId, eventData.CommandId);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(_commandMethod, eventData.ExecuteMethod);

                ExecutedCalled = true;
            }
        }

        protected class Singularity
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Type { get; set; }
        }

        protected class Brane
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Type { get; set; }
        }

        public class UniverseContext : PoolableDbContext
        {
            public UniverseContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Singularity>()
                    .HasData(
                        new Singularity
                        {
                            Id = 77, Type = "Black Hole"
                        },
                        new Singularity
                        {
                            Id = 88, Type = "Bing Bang"
                        });

                modelBuilder
                    .Entity<Brane>()
                    .HasData(
                        new Brane
                        {
                            Id = 77, Type = "Black Hole?"
                        },
                        new Brane
                        {
                            Id = 88, Type = "Bing Bang?"
                        });
            }
        }

        protected void AssertSql(string expected, string actual)
            => Assert.Equal(
                expected,
                actual.Replace("\r", string.Empty).Replace("\n", " "));

        protected (DbContext, TInterceptor) CreateContext<TInterceptor>(bool inject)
            where TInterceptor : class, IDbCommandInterceptor, new()
        {
            var interceptor = inject ? null : new TInterceptor();

            var context = inject
                ? CreateContext(null, typeof(TInterceptor))
                : CreateContext(interceptor);

            if (inject)
            {
                interceptor = (TInterceptor)context.GetService<IEnumerable<IDbCommandInterceptor>>().Single();
            }

            return (context, interceptor);
        }

        public UniverseContext CreateContext(
            IDbCommandInterceptor appInterceptor,
            params Type[] injectedInterceptorTypes)
            => new UniverseContext(
                Fixture.AddRelationalOptions(
                    b =>
                    {
                        if (appInterceptor != null)
                        {
                            b.CommandInterceptor(appInterceptor);
                        }
                    },
                    injectedInterceptorTypes));

        public abstract class InterceptionFixtureBase : SharedStoreFixtureBase<UniverseContext>
        {
            protected virtual IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                Type[] injectedInterceptorTypes)
            {
                foreach (var interceptorType in injectedInterceptorTypes)
                {
                    serviceCollection.AddScoped(typeof(IDbCommandInterceptor), interceptorType);
                }

                return serviceCollection;
            }

            public abstract DbContextOptions AddRelationalOptions(
                Action<RelationalDbContextOptionsBuilder<TBuilder, TExtension>> relationalBuilder,
                Type[] injectedInterceptorTypes);
        }
    }
}
