// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGRelationalConnection : RelationalConnection, IXGRelationalConnection
    {
        private readonly IXGConnectionStringOptionsValidator _xgConnectionStringOptionsValidator;
        private const string NoBackslashEscapes = "NO_BACKSLASH_ESCAPES";

        private readonly XGOptionsExtension _xgOptionsExtension;
        private DbDataSource _dataSource;

        public XGRelationalConnection(
            RelationalConnectionDependencies dependencies,
            IXGConnectionStringOptionsValidator xgConnectionStringOptionsValidator,
            IXGOptions xgSingletonOptions)
            : this(
                dependencies,
                xgConnectionStringOptionsValidator,
                GetEffectiveDataSource(xgSingletonOptions, dependencies.ContextOptions))
        {
        }

        public XGRelationalConnection(
            RelationalConnectionDependencies dependencies,
            IXGConnectionStringOptionsValidator xgConnectionStringOptionsValidator,
            DbDataSource dataSource)
            : base(dependencies)
        {
            _xgOptionsExtension = dependencies.ContextOptions.FindExtension<XGOptionsExtension>() ??
                                     new XGOptionsExtension();
            _xgConnectionStringOptionsValidator = xgConnectionStringOptionsValidator;

            if (dataSource is not null)
            {
                _xgConnectionStringOptionsValidator.EnsureMandatoryOptions(dataSource);

                base.SetDbConnection(null, false);
                base.ConnectionString = null;

                _dataSource = dataSource;
            }
            else if (base.ConnectionString is { } connectionString)
            {
                // This branch works for both: connections and connection strings, because base.ConnectionString handles both cases
                // appropriately.
                if (_xgConnectionStringOptionsValidator.EnsureMandatoryOptions(ref connectionString))
                {
                    try
                    {
                        base.ConnectionString = connectionString;
                    }
                    catch (Exception e)
                    {
                        _xgConnectionStringOptionsValidator.ThrowException(e);
                    }
                }
            }
        }

        /// <summary>
        /// We allow users to either explicitly set a DbDataSource using our `XGOptionsExtensions` or by adding it as a service via DI
        /// (`ApplicationServiceProvider`).
        /// We don't set a DI injected service to the `XGOption.DbDataSource` property, because it might get cached by the service
        /// collection cache, since no relevant property might have changed in the `XGOptionsExtension` instance. If we would create
        /// a similar DbContext instance with a different service collection later, EF Core would provide us with the *same* `XGOptions`
        /// instance (that was cached before) and we would use the old `DbDataSource` instance that we retrieved from the old
        /// `ApplicationServiceProvider`.
        /// Therefore, we check the `IXGOptions.DbDataSource` property and the current `ApplicationServiceProvider` at the time we
        /// actually need the instance.
        /// </summary>
        protected static DbDataSource GetEffectiveDataSource(IXGOptions xgSingletonOptions, IDbContextOptions contextOptions)
            => xgSingletonOptions.DataSource ??
               contextOptions.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider?.GetService<DbDataSource>();

        private bool IsMasterConnection { get; set; }

        protected override DbConnection CreateDbConnection()
            => _dataSource is not null
                ? _dataSource.CreateConnection()
                : new XGConnection(AddConnectionStringOptions(new XGConnectionStringBuilder(ConnectionString!)).ConnectionString);

        public override string ConnectionString
        {
            get => _dataSource is null
                ? base.ConnectionString
                : _dataSource.ConnectionString;
            set
            {
                _xgConnectionStringOptionsValidator.EnsureMandatoryOptions(ref value);
                base.ConnectionString = value;

                _dataSource = null;
            }
        }

        public override void SetDbConnection(DbConnection value, bool contextOwnsConnection)
        {
            _xgConnectionStringOptionsValidator.EnsureMandatoryOptions(value);

            base.SetDbConnection(value, contextOwnsConnection);
        }

        [AllowNull]
        public new virtual XGConnection DbConnection
        {
            get => (XGConnection)base.DbConnection;
            set
            {
                base.DbConnection = value;

                _dataSource = null;
            }
        }

        public virtual DbDataSource DbDataSource
        {
            get => _dataSource;
            set
            {
                _xgConnectionStringOptionsValidator.EnsureMandatoryOptions(value);

                if (value is not null)
                {
                    DbConnection = null;
                    ConnectionString = null;
                }

                _dataSource = value;
            }
        }

        public virtual IXGRelationalConnection CreateMasterConnection()
        {
            if (Dependencies.ContextOptions.FindExtension<XGOptionsExtension>() is not { } xgOptions)
            {
                throw new InvalidOperationException($"{nameof(XGOptionsExtension)} not found in {nameof(CreateMasterConnection)}");
            }

            // Add master connection specific options.
            var csb = new XGConnectionStringBuilder(ConnectionString!)
            {
                Database = string.Empty
            };

            csb = AddConnectionStringOptions(csb);

            var masterConnectionString = csb.ConnectionString;

            // Apply modified connection string.
            var masterXGOptions = _dataSource is not null
                ? xgOptions.WithConnection(((XGConnection)CreateDbConnection()).CloneWith(masterConnectionString), owned: true)
                : xgOptions.Connection is null
                    ? xgOptions.WithConnectionString(masterConnectionString)
                    : xgOptions.WithConnection(DbConnection.CloneWith(masterConnectionString), owned: true);

            var optionsBuilder = new DbContextOptionsBuilder();
            var optionsBuilderInfrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;

            optionsBuilderInfrastructure.AddOrUpdateExtension(masterXGOptions);

            return CreateMasterConnectionCore(optionsBuilder, _xgConnectionStringOptionsValidator);
        }

        protected virtual IXGRelationalConnection CreateMasterConnectionCore(
            DbContextOptionsBuilder optionsBuilder,
            IXGConnectionStringOptionsValidator xgConnectionStringOptionsValidator)
            => new XGRelationalConnection(
                Dependencies with { ContextOptions = optionsBuilder.Options },
                xgConnectionStringOptionsValidator,
                dataSource: null)
            {
                IsMasterConnection = true
            };

        protected virtual XGConnectionStringBuilder AddConnectionStringOptions(XGConnectionStringBuilder builder)
        {
            return builder;
        }

        protected override bool SupportsAmbientTransactions => true;

        // CHECK: Is this obsolete or has it been moved somewhere else?
        // public override bool IsMultipleActiveResultSetsEnabled => false;

        public override void EnlistTransaction(Transaction transaction)
        {
            try
            {
                base.EnlistTransaction(transaction);
            }
            catch (Exception e)
            {
                if (e.Message == "Already enlisted in a Transaction.")
                {
                    // Return expected exception type.
                    throw new InvalidOperationException(e.Message, e);
                }

                throw;
            }
        }

        public override bool Open(bool errorsExpected = false)
        {
            var result = base.Open(errorsExpected);

            if (result)
            {
                if (_xgOptionsExtension.UpdateSqlModeOnOpen &&
                    _xgOptionsExtension.NoBackslashEscapes &&
                    !IsMasterConnection)
                {
                    AddSqlMode(NoBackslashEscapes);
                }
            }

            return result;
        }

        public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            var result = await base.OpenAsync(cancellationToken, errorsExpected)
                .ConfigureAwait(false);

            if (result)
            {
                if (_xgOptionsExtension.UpdateSqlModeOnOpen &&
                    _xgOptionsExtension.NoBackslashEscapes &&
                    !IsMasterConnection)
                {
                    await AddSqlModeAsync(NoBackslashEscapes, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return result;
        }

        public virtual void AddSqlMode(string mode)
            => ExecuteNonQuery($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', '{mode}');");

        public virtual Task AddSqlModeAsync(string mode, CancellationToken cancellationToken = default)
            => ExecuteNonQueryAsync($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', '{mode}');", cancellationToken);

        public virtual void RemoveSqlMode(string mode)
            => ExecuteNonQuery($@"SET SESSION sql_mode = REPLACE(@@sql_mode, '{mode}', '');");

        public virtual Task RemoveSqlModeAsync(string mode, CancellationToken cancellationToken = default)
            => ExecuteNonQueryAsync($@"SET SESSION sql_mode = REPLACE(@@sql_mode, '{mode}', '');", cancellationToken);

        protected virtual void ExecuteNonQuery(string sql)
        {
            using var command = DbConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        protected virtual async Task ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default)
        {
            var command = DbConnection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
