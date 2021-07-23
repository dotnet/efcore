// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class CommandInterceptionTestBase : InterceptionTestBase
    {
        protected CommandInterceptionTestBase(InterceptionFixtureBase fixture)
            : base(fixture)
        {
        }

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
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole", results[0].Type);
                Assert.Equal("Bing Bang", results[1].Type);

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
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
                const string sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var result = async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject);

                Assert.Equal(1, Convert.ToInt32(result));

                AssertNormalOutcome(context, interceptor, async);

                AssertSql(sql, interceptor.CommandText);

                AssertExecutedEvents(listener);
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
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 77");

                    using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                    var result = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.Equal(1, result);

                    AssertNormalOutcome(context, interceptor, async);

                    AssertSql(nonQuery, interceptor.CommandText);

                    AssertExecutedEvents(listener);
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
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                Assert.Equal(3, results.Count);
                Assert.Equal(977, results[0].Id);
                Assert.Equal(988, results[1].Id);
                Assert.Equal(999, results[2].Id);
                Assert.Equal("<977>", results[0].Type);
                Assert.Equal("<988>", results[1].Type);
                Assert.Equal("<999>", results[2].Type);

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
            }

            return interceptor.CommandText;
        }

        protected class SuppressingReaderCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                base.ReaderExecuting(command, eventData, result);

                return InterceptionResult<DbDataReader>.SuppressWithResult(new FakeDbDataReader());
            }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

                return new ValueTask<InterceptionResult<DbDataReader>>(
                    InterceptionResult<DbDataReader>.SuppressWithResult(new FakeDbDataReader()));
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_query_to_suppress_command_creation(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<SuppressingCreateCommandInterceptor>(inject);
            using (context)
            {
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole", results[0].Type);
                Assert.Equal("Bing Bang", results[1].Type);

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
            }
        }

        protected class SuppressingCreateCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingCreateCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbCommand> CommandCreating(
                CommandCorrelatedEventData eventData,
                InterceptionResult<DbCommand> result)
            {
                base.CommandCreating(eventData, result);

                var wrappedCommand = eventData.Connection.CreateCommand();

                return InterceptionResult<DbCommand>.SuppressWithResult(new WrappingDbCommand(wrappedCommand));
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                Assert.IsType<WrappingDbCommand>(command);

                return base.ReaderExecuting(command, eventData, result);
            }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                Assert.IsType<WrappingDbCommand>(command);

                return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
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
                const string sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var result = async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject);

                Assert.Equal(SuppressingScalarCommandInterceptor.InterceptedResult, result);

                AssertNormalOutcome(context, interceptor, async);

                AssertSql(sql, interceptor.CommandText);

                AssertExecutedEvents(listener);
            }
        }

        protected class SuppressingScalarCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public const string InterceptedResult = "Bet you weren't expecting a string!";

            public override InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                base.ScalarExecuting(command, eventData, result);

                return InterceptionResult<object>.SuppressWithResult(InterceptedResult);
            }

            public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                base.ScalarExecutingAsync(command, eventData, result, cancellationToken);

                return new ValueTask<InterceptionResult<object>>(InterceptionResult<object>.SuppressWithResult(InterceptedResult));
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
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 77");

                    using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                    var result = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.Equal(2, result);

                    AssertNormalOutcome(context, interceptor, async);

                    AssertSql(nonQuery, interceptor.CommandText);

                    AssertExecutedEvents(listener);
                }
            }
        }

        protected class SuppressingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public SuppressingNonQueryCommandInterceptor()
                : base(DbCommandMethod.ExecuteNonQuery)
            {
            }

            public override InterceptionResult<int> NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result)
            {
                base.NonQueryExecuting(command, eventData, result);

                return InterceptionResult<int>.SuppressWithResult(2);
            }

            public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

                return new ValueTask<InterceptionResult<int>>(InterceptionResult<int>.SuppressWithResult(2));
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
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole?", results[0].Type);
                Assert.Equal("Bing Bang?", results[1].Type);

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
            }

            return interceptor.CommandText;
        }

        protected class MutatingReaderCommandInterceptor : CommandInterceptorBase
        {
            public MutatingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                MutateQuery(command);

                return base.ReaderExecuting(command, eventData, result);
            }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
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
                const string sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var result = async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject);

                Assert.Equal(2, Convert.ToInt32(result));

                AssertNormalOutcome(context, interceptor, async);

                AssertSql(MutatingScalarCommandInterceptor.MutatedSql, interceptor.CommandText);

                AssertExecutedEvents(listener);
            }
        }

        protected class MutatingScalarCommandInterceptor : CommandInterceptorBase
        {
            public MutatingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public const string MutatedSql = "SELECT 2";

            public override InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                command.CommandText = MutatedSql;

                return base.ScalarExecuting(command, eventData, result);
            }

            public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
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
            var interceptor = new MutatingNonQueryCommandInterceptor(this);
            var context = inject ? CreateContext(null, interceptor) : CreateContext(interceptor);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 77");

                    using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                    var result = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.Equal(0, result);

                    AssertNormalOutcome(context, interceptor, async);

                    AssertSql(interceptor.MutatedSql, interceptor.CommandText);

                    AssertExecutedEvents(listener);
                }
            }
        }

        protected class MutatingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            public readonly string MutatedSql;

            public MutatingNonQueryCommandInterceptor(CommandInterceptionTestBase testBase)
                : base(DbCommandMethod.ExecuteNonQuery)
            {
                MutatedSql =
                    testBase.NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 78");
            }

            public override InterceptionResult<int> NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result)
            {
                command.CommandText = MutatedSql;

                return base.NonQueryExecuting(command, eventData, result);
            }

            public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
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
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                Assert.Equal(2, results.Count);
                Assert.Equal(77, results[0].Id);
                Assert.Equal(88, results[1].Id);
                Assert.Equal("Black Hole?", results[0].Type);
                Assert.Equal("Bing Bang?", results[1].Type);

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
            }

            return interceptor.CommandText;
        }

        protected class QueryReplacingReaderCommandInterceptor : CommandInterceptorBase
        {
            public QueryReplacingReaderCommandInterceptor()
                : base(DbCommandMethod.ExecuteReader)
            {
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                base.ReaderExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<DbDataReader>.SuppressWithResult(CreateNewCommand(command).ExecuteReader());
            }

            public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<DbDataReader>.SuppressWithResult(
                    await CreateNewCommand(command).ExecuteReaderAsync(cancellationToken));
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
                const string sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var result = async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject);

                Assert.Equal(2, Convert.ToInt32(result));

                AssertNormalOutcome(context, interceptor, async);

                AssertSql(sql, interceptor.CommandText);

                AssertExecutedEvents(listener);
            }
        }

        protected class QueryReplacingScalarCommandInterceptor : CommandInterceptorBase
        {
            public QueryReplacingScalarCommandInterceptor()
                : base(DbCommandMethod.ExecuteScalar)
            {
            }

            public override InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                base.ScalarExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<object>.SuppressWithResult(CreateNewCommand(command).ExecuteScalar());
            }

            public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<object>.SuppressWithResult(await CreateNewCommand(command).ExecuteScalarAsync(cancellationToken));
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
            var interceptor = new QueryReplacingNonQueryCommandInterceptor(this);
            var context = inject ? CreateContext(null, interceptor) : CreateContext(interceptor);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 78");

                    using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                    var result = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.Equal(1, result);

                    AssertNormalOutcome(context, interceptor, async);

                    AssertSql(nonQuery, interceptor.CommandText);

                    AssertExecutedEvents(listener);
                }
            }
        }

        protected class QueryReplacingNonQueryCommandInterceptor : CommandInterceptorBase
        {
            private readonly string commandText;

            public QueryReplacingNonQueryCommandInterceptor(CommandInterceptionTestBase testBase)
                : base(DbCommandMethod.ExecuteNonQuery)
            {
                commandText = testBase.NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 77");
            }

            public override InterceptionResult<int> NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result)
            {
                base.NonQueryExecuting(command, eventData, result);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<int>.SuppressWithResult(CreateNewCommand(command).ExecuteNonQuery());
            }

            public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

                // Note: this DbCommand will not get disposed...can be problematic on some providers
                return InterceptionResult<int>.SuppressWithResult(await CreateNewCommand(command).ExecuteNonQueryAsync(cancellationToken));
            }

            private DbCommand CreateNewCommand(DbCommand command)
            {
                var newCommand = command.Connection.CreateCommand();
                newCommand.Transaction = command.Transaction;
                newCommand.CommandText = commandText;

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
                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

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

                AssertNormalOutcome(context, interceptor, async);

                AssertExecutedEvents(listener);
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

            public override ValueTask<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                base.ReaderExecutedAsync(command, eventData, result, cancellationToken);

                return new ValueTask<DbDataReader>(new CompositeFakeDbDataReader(result, new FakeDbDataReader()));
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

            public override int FieldCount
                => _firstReader.FieldCount;

            public override int RecordsAffected
                => _firstReader.RecordsAffected + _secondReader.RecordsAffected;

            public override bool HasRows
                => _firstReader.HasRows || _secondReader.HasRows;

            public override bool IsClosed
                => _firstReader.IsClosed;

            public override int Depth
                => _firstReader.Depth;

            public override string GetDataTypeName(int ordinal)
                => _firstReader.GetDataTypeName(ordinal);

            public override Type GetFieldType(int ordinal)
                => _firstReader.GetFieldType(ordinal);

            public override string GetName(int ordinal)
                => _firstReader.GetName(ordinal);

            public override bool NextResult()
                => _firstReader.NextResult() || _secondReader.NextResult();

            public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
                => await _firstReader.NextResultAsync(cancellationToken) || await _secondReader.NextResultAsync(cancellationToken);

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
                const string sql = "SELECT 1";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                var result = async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject);

                Assert.Equal(ResultReplacingScalarCommandInterceptor.InterceptedResult, result);

                AssertNormalOutcome(context, interceptor, async);

                AssertSql(sql, interceptor.CommandText);

                AssertExecutedEvents(listener);
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

            public override ValueTask<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result,
                CancellationToken cancellationToken = default)
            {
                base.ScalarExecutedAsync(command, eventData, result, cancellationToken);

                return new ValueTask<object>(InterceptedResult);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_to_replace_result(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<ResultReplacingNonQueryCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 78");

                    using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
                    var result = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.Equal(7, result);

                    AssertNormalOutcome(context, interceptor, async);

                    AssertSql(nonQuery, interceptor.CommandText);

                    AssertExecutedEvents(listener);
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

            public override async ValueTask<int> NonQueryExecutedAsync(
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
        public virtual async Task Intercept_query_that_throws(bool async, bool inject)
        {
            string badSql = NormalizeDelimitersInRawString("SELECT * FROM [TheVoid]");

            var (context, interceptor) = CreateContext<PassiveReaderCommandInterceptor>(inject);
            using (context)
            {
                try
                {
                    _ = async
                        ? await context.Set<Singularity>().FromSqlRaw(badSql).ToListAsync()
                        : context.Set<Singularity>().FromSqlRaw(badSql).ToList();

                    Assert.False(true);
                }
                catch (Exception exception)
                {
                    Assert.Same(interceptor.Exception, exception);
                }

                AssertErrorOutcome(context, interceptor, async);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_scalar_that_throws(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<PassiveScalarCommandInterceptor>(inject);
            using (context)
            {
                const string sql = "SELECT Won";

                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append(sql).Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                try
                {
                    _ = async
                        ? await command.ExecuteScalarAsync(commandParameterObject)
                        : command.ExecuteScalar(commandParameterObject);

                    Assert.False(true);
                }
                catch (Exception exception)
                {
                    Assert.Same(interceptor.Exception, exception);
                }

                AssertErrorOutcome(context, interceptor, async);

                AssertSql(sql, interceptor.CommandText);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_non_query_that_throws(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<PassiveNonQueryCommandInterceptor>(inject);
            using (context)
            {
                string nonQuery = NormalizeDelimitersInRawString("DELETE FROM [TheVoid] WHERE [Id] = 555");

                try
                {
                    _ = async
                        ? await context.Database.ExecuteSqlRawAsync(nonQuery)
                        : context.Database.ExecuteSqlRaw(nonQuery);

                    Assert.False(true);
                }
                catch (Exception exception)
                {
                    Assert.Same(interceptor.Exception, exception);
                }

                AssertErrorOutcome(context, interceptor, async);

                AssertSql(nonQuery, interceptor.CommandText);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public virtual async Task Intercept_query_to_throw(bool async, bool inject)
        {
            var (context, interceptor) = CreateContext<ThrowingReaderCommandInterceptor>(inject);
            using (context)
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
            var (context, interceptor) = CreateContext<ThrowingReaderCommandInterceptor>(inject);
            using (context)
            {
                var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append("SELECT 1").Build();
                var connection = context.GetService<IRelationalConnection>();
                var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

                var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

                var exception = async
                    ? await Assert.ThrowsAsync<Exception>(() => command.ExecuteScalarAsync(commandParameterObject))
                    : Assert.Throws<Exception>(() => command.ExecuteScalar(commandParameterObject));

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
            var (context, interceptor) = CreateContext<ThrowingReaderCommandInterceptor>(inject);
            using (context)
            {
                using (context.Database.BeginTransaction())
                {
                    string nonQuery =
                        NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 77");

                    var exception = async
                        ? await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteSqlRawAsync(nonQuery))
                        : Assert.Throws<Exception>(() => context.Database.ExecuteSqlRaw(nonQuery));

                    Assert.Equal("Bang!", exception.Message);
                }
            }
        }

        protected class ThrowingReaderCommandInterceptor : DbCommandInterceptor
        {
            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                throw new Exception("Bang!");
            }

            public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                throw new Exception("Bang!");
            }

            public override InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                throw new Exception("Bang!");
            }

            public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                throw new Exception("Bang!");
            }

            public override InterceptionResult<int> NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result)
            {
                throw new Exception("Bang!");
            }

            public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
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
            var injectedInterceptor = new MutatingReaderCommandInterceptor();
            using var context = CreateContext(appInterceptor, injectedInterceptor);
            await TestCompoisteQueryInterceptors(context, appInterceptor, injectedInterceptor, async);
        }

        private static async Task TestCompoisteQueryInterceptors(
            UniverseContext context,
            ResultReplacingReaderCommandInterceptor interceptor1,
            MutatingReaderCommandInterceptor interceptor2,
            bool async)
        {
            var results = async
                ? await context.Set<Singularity>().ToListAsync()
                : context.Set<Singularity>().ToList();

            AssertCompositeResults(results);

            AssertNormalOutcome(context, interceptor1, async);
            AssertNormalOutcome(context, interceptor2, async);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_scalar_with_one_app_and_one_injected_interceptor(bool async)
        {
            using var context = CreateContext(
                new ResultReplacingScalarCommandInterceptor(),
                new MutatingScalarCommandInterceptor());
            await TestCompositeScalarInterceptors(context, async);
        }

        private static async Task TestCompositeScalarInterceptors(UniverseContext context, bool async)
        {
            var command = context.GetService<IRelationalCommandBuilderFactory>().Create().Append("SELECT 1").Build();
            var connection = context.GetService<IRelationalConnection>();
            var logger = context.GetService<IRelationalCommandDiagnosticsLogger>();

            var commandParameterObject = new RelationalCommandParameterObject(connection, null, null, context, logger);

            Assert.Equal(
                ResultReplacingScalarCommandInterceptor.InterceptedResult,
                async
                    ? await command.ExecuteScalarAsync(commandParameterObject)
                    : command.ExecuteScalar(commandParameterObject));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_one_app_and_one_injected_interceptor(bool async)
        {
            using var context = CreateContext(
                new ResultReplacingNonQueryCommandInterceptor(),
                new MutatingNonQueryCommandInterceptor(this));
            await TestCompositeNonQueryInterceptors(context, async);
        }

        private async Task TestCompositeNonQueryInterceptors(UniverseContext context, bool async)
        {
            using (context.Database.BeginTransaction())
            {
                string nonQuery =
                    NormalizeDelimitersInRawString("DELETE FROM [Singularity] WHERE [Id] = 78");

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
            var injectedInterceptor1 = new MutatingReaderCommandInterceptor();
            var injectedInterceptor2 = new ResultReplacingReaderCommandInterceptor();

            using var context = CreateContext(null, injectedInterceptor1, injectedInterceptor2);
            await TestCompoisteQueryInterceptors(context, injectedInterceptor2, injectedInterceptor1, async);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_scalar_with_two_injected_interceptors(bool async)
        {
            using var context = CreateContext(
                null,
                new MutatingScalarCommandInterceptor(), new ResultReplacingScalarCommandInterceptor());
            await TestCompositeScalarInterceptors(context, async);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_with_two_injected_interceptors(bool async)
        {
            using var context = CreateContext(
                null,
                new MutatingNonQueryCommandInterceptor(this), new ResultReplacingNonQueryCommandInterceptor());
            await TestCompositeNonQueryInterceptors(context, async);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_query_with_explicitly_composed_app_interceptor(bool async)
        {
            using var context = CreateContext(
                new IInterceptor[] { new MutatingReaderCommandInterceptor(), new ResultReplacingReaderCommandInterceptor() });
            var results = async
                ? await context.Set<Singularity>().ToListAsync()
                : context.Set<Singularity>().ToList();

            AssertCompositeResults(results);
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
            using var context = CreateContext(
                new IInterceptor[] { new MutatingScalarCommandInterceptor(), new ResultReplacingScalarCommandInterceptor() });
            await TestCompositeScalarInterceptors(context, async);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_non_query_with_explicitly_composed_app_interceptor(bool async)
        {
            using var context = CreateContext(
                new IInterceptor[] { new MutatingNonQueryCommandInterceptor(this), new ResultReplacingNonQueryCommandInterceptor() });
            await TestCompositeNonQueryInterceptors(context, async);
        }

        private class WrappingDbCommand : DbCommand
        {
            private readonly DbCommand _command;

            public WrappingDbCommand(DbCommand command)
                => _command = command;

            public override void Cancel()
                => _command.Cancel();

            public override int ExecuteNonQuery()
                => _command.ExecuteNonQuery();

            public override object ExecuteScalar()
                => _command.ExecuteScalar();

            public override void Prepare()
                => _command.Prepare();

            public override string CommandText
            {
                get => _command.CommandText;
                set => _command.CommandText = value;
            }

            public override int CommandTimeout
            {
                get => _command.CommandTimeout;
                set => _command.CommandTimeout = value;
            }

            public override CommandType CommandType
            {
                get => _command.CommandType;
                set => _command.CommandType = value;
            }

            public override UpdateRowSource UpdatedRowSource
            {
                get => _command.UpdatedRowSource;
                set => _command.UpdatedRowSource = value;
            }

            protected override DbConnection DbConnection
            {
                get => _command.Connection;
                set => _command.Connection = value;
            }

            protected override DbParameterCollection DbParameterCollection
                => _command.Parameters;

            protected override DbTransaction DbTransaction
            {
                get => _command.Transaction;
                set => _command.Transaction = value;
            }

            public override bool DesignTimeVisible
            {
                get => _command.DesignTimeVisible;
                set => _command.DesignTimeVisible = value;
            }

            protected override DbParameter CreateDbParameter()
                => _command.CreateParameter();

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
                => _command.ExecuteReader();
        }

        private class FakeDbDataReader : DbDataReader
        {
            private int _index;

            private readonly int[] _ints = { 977, 988, 999 };

            private readonly string[] _strings = { "<977>", "<988>", "<999>" };
            public override int FieldCount { get; }
            public override int RecordsAffected { get; }
            public override bool HasRows { get; }
            public override bool IsClosed { get; }
            public override int Depth { get; }

            public override bool Read()
                => _index++ < _ints.Length;

            public override int GetInt32(int ordinal)
                => _ints[_index - 1];

            public override bool IsDBNull(int ordinal)
                => false;

            public override string GetString(int ordinal)
                => _strings[_index - 1];

            public override bool GetBoolean(int ordinal)
                => throw new NotImplementedException();

            public override byte GetByte(int ordinal)
                => throw new NotImplementedException();

            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                => throw new NotImplementedException();

            public override char GetChar(int ordinal)
                => throw new NotImplementedException();

            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                => throw new NotImplementedException();

            public override string GetDataTypeName(int ordinal)
                => throw new NotImplementedException();

            public override DateTime GetDateTime(int ordinal)
                => throw new NotImplementedException();

            public override decimal GetDecimal(int ordinal)
                => throw new NotImplementedException();

            public override double GetDouble(int ordinal)
                => throw new NotImplementedException();

            public override Type GetFieldType(int ordinal)
                => throw new NotImplementedException();

            public override float GetFloat(int ordinal)
                => throw new NotImplementedException();

            public override Guid GetGuid(int ordinal)
                => throw new NotImplementedException();

            public override short GetInt16(int ordinal)
                => throw new NotImplementedException();

            public override long GetInt64(int ordinal)
                => throw new NotImplementedException();

            public override string GetName(int ordinal)
                => throw new NotImplementedException();

            public override int GetOrdinal(string name)
                => throw new NotImplementedException();

            public override object GetValue(int ordinal)
                => throw new NotImplementedException();

            public override int GetValues(object[] values)
                => throw new NotImplementedException();

            public override object this[int ordinal]
                => throw new NotImplementedException();

            public override object this[string name]
                => throw new NotImplementedException();

            public override bool NextResult()
                => throw new NotImplementedException();

            public override IEnumerator GetEnumerator()
                => throw new NotImplementedException();
        }

        private static void AssertNormalOutcome(DbContext context, CommandInterceptorBase interceptor, bool async)
        {
            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
            Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
            Assert.True(interceptor.ExecutingCalled);
            Assert.True(interceptor.ExecutedCalled);
            Assert.False(interceptor.FailedCalled);
            Assert.Same(context, interceptor.Context);
        }

        private static void AssertErrorOutcome(DbContext context, CommandInterceptorBase interceptor, bool async)
        {
            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
            Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
            Assert.True(interceptor.ExecutingCalled);
            Assert.False(interceptor.ExecutedCalled);
            Assert.True(interceptor.FailedCalled);
            Assert.Same(context, interceptor.Context);
        }

        private static void AssertExecutedEvents(ITestDiagnosticListener listener)
            => listener.AssertEventsInOrder(
                RelationalEventId.CommandExecuting.Name,
                RelationalEventId.CommandExecuted.Name);

        protected static void AssertSql(string expected, string actual)
            => Assert.Equal(
                expected,
                actual.Replace("\r", string.Empty).Replace("\n", " "));

        protected abstract class CommandInterceptorBase : IDbCommandInterceptor
        {
            private readonly DbCommandMethod _commandMethod;

            protected CommandInterceptorBase(DbCommandMethod commandMethod)
            {
                _commandMethod = commandMethod;
            }

            public DbContext Context { get; set; }
            public Exception Exception { get; set; }
            public string CommandText { get; set; }
            public Guid CommandId { get; set; }
            public Guid ConnectionId { get; set; }
            public bool AsyncCalled { get; set; }
            public bool SyncCalled { get; set; }
            public bool ExecutingCalled { get; set; }
            public bool ExecutedCalled { get; set; }
            public bool FailedCalled { get; set; }
            public bool CreatingCalled { get; set; }
            public bool CreatedCalled { get; set; }

            public virtual InterceptionResult<DbCommand> CommandCreating(
                CommandCorrelatedEventData eventData,
                InterceptionResult<DbCommand> result)
            {
                AssertCreating(eventData);

                return result;
            }

            public virtual DbCommand CommandCreated(
                CommandEndEventData eventData,
                DbCommand result)
            {
                AssertCreated(result, eventData);

                return result;
            }

            public virtual InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual InterceptionResult<int> NonQueryExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertExecuting(command, eventData);

                return result;
            }

            public virtual ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return new ValueTask<InterceptionResult<DbDataReader>>(result);
            }

            public virtual ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return new ValueTask<InterceptionResult<object>>(result);
            }

            public virtual ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuting(command, eventData);

                return new ValueTask<InterceptionResult<int>>(result);
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

            public virtual ValueTask<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return new ValueTask<DbDataReader>(result);
            }

            public virtual ValueTask<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return new ValueTask<object>(result);
            }

            public virtual ValueTask<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertExecuted(command, eventData);

                return new ValueTask<int>(result);
            }

            public void CommandFailed(
                DbCommand command,
                CommandErrorEventData eventData)
            {
                Assert.False(eventData.IsAsync);
                SyncCalled = true;
                AssertFailed(command, eventData);
            }

            public Task CommandFailedAsync(
                DbCommand command,
                CommandErrorEventData eventData,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertFailed(command, eventData);

                return Task.CompletedTask;
            }

            public InterceptionResult DataReaderDisposing(
                DbCommand command,
                DataReaderDisposingEventData eventData,
                InterceptionResult result)
            {
                Assert.NotNull(eventData.DataReader);
                Assert.Same(Context, eventData.Context);
                Assert.Equal(CommandText, command.CommandText);
                Assert.Equal(CommandId, eventData.CommandId);
                Assert.Equal(ConnectionId, eventData.ConnectionId);

                return result;
            }

            protected virtual void AssertExecuting(DbCommand command, CommandEventData eventData)
            {
                Assert.NotNull(eventData.Context);
                Assert.NotEqual(default, eventData.CommandId);
                Assert.NotEqual(default, eventData.ConnectionId);
                Assert.Equal(CommandId, eventData.CommandId);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
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

            protected virtual void AssertCreating(CommandCorrelatedEventData eventData)
            {
                Assert.NotNull(eventData.Context);
                Assert.NotEqual(default, eventData.CommandId);
                Assert.NotEqual(default, eventData.ConnectionId);
                Assert.Equal(_commandMethod, eventData.ExecuteMethod);

                Context = eventData.Context;
                CommandId = eventData.CommandId;
                ConnectionId = eventData.ConnectionId;
                CreatingCalled = true;
            }

            protected virtual void AssertCreated(DbCommand command, CommandEndEventData eventData)
            {
                Assert.NotNull(command);
                Assert.Same(Context, eventData.Context);
                Assert.Equal(CommandId, eventData.CommandId);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(_commandMethod, eventData.ExecuteMethod);

                CreatedCalled = true;
            }

            protected virtual void AssertFailed(DbCommand command, CommandErrorEventData eventData)
            {
                Assert.Same(Context, eventData.Context);
                Assert.Equal(CommandText, command.CommandText);
                Assert.Equal(CommandId, eventData.CommandId);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(_commandMethod, eventData.ExecuteMethod);
                Assert.NotNull(eventData.Exception);

                Exception = eventData.Exception;
                FailedCalled = true;
            }
        }

        private string NormalizeDelimitersInRawString(string sql)
            => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimitersInRawString(sql);
    }
}
