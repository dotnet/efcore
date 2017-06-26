// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	/// <summary>
	///     This API supports the Entity Framework Core infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public class SqlServerSchemaCreator : RelationalSchemaCreator
	{
		private readonly ISqlServerConnection _connection;
		private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

		public SqlServerSchemaCreator(
			[NotNull] RelationalSchemaCreatorDependencies dependencies,
			[NotNull] ISqlServerConnection connection,
			[NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
			: base(dependencies)
		{
			_connection = connection;
			_rawSqlCommandBuilder = rawSqlCommandBuilder;
		}

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public override void Create()
		{
			Annotation annotation = Dependencies.Model.FindAnnotation("Relational:DefaultSchema") as Annotation;
			if (annotation != null && annotation.Value != null && annotation.Value is string && !string.IsNullOrEmpty((string)annotation.Value))
			{
				string commandText = string.Format("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}') BEGIN EXEC('CREATE SCHEMA {0}') END; ", annotation.Value);
				using (DbCommand dbCommand = _connection.DbConnection.CreateCommand())
				{
					dbCommand.CommandText = commandText;
					_connection.Open();
					dbCommand.ExecuteNonQuery();
					_connection.Close();
				}
			}
		}

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			Annotation annotation = Dependencies.Model.FindAnnotation("Relational:DefaultSchema") as Annotation;
			if (annotation != null && annotation.Value != null && annotation.Value is string && !string.IsNullOrEmpty((string)annotation.Value))
			{
				string commandText = string.Format("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}') BEGIN EXEC('CREATE SCHEMA {0}') END; ", annotation.Value);
				using (DbCommand dbCommand = _connection.DbConnection.CreateCommand())
				{
					dbCommand.CommandText = commandText;
					await _connection.OpenAsync();
					await dbCommand.ExecuteNonQueryAsync();
					_connection.Close();
				}
			}
		}
		
		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public override bool Exists()
			=> Exists(retryOnNotExists: false);

		private bool Exists(bool retryOnNotExists)
			=> Dependencies.ExecutionStrategyFactory.Create().Execute(DateTime.UtcNow + RetryTimeout, giveUp =>
				{
					Annotation annotation = Dependencies.Model.FindAnnotation("Relational:DefaultSchema") as Annotation;
					if (annotation != null && annotation.Value != null && annotation.Value is string && !string.IsNullOrEmpty((string)annotation.Value))
					{
						string commandText = string.Format("SELECT schema_id FROM sys.schemas WHERE name = '{0}'", annotation.Value);
						using (DbCommand dbCommand = _connection.DbConnection.CreateCommand())
						{
							dbCommand.CommandText = commandText;
							_connection.Open();
							object result = dbCommand.ExecuteScalar();
							_connection.Close();
							if (result != null && result is int && (int)result > 0)
								return true;
							else
								return false;
						}
					}
					else
						return true;
				});

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
			=> ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

		private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
			=> Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
				{
					Annotation annotation = Dependencies.Model.FindAnnotation("Relational:DefaultSchema") as Annotation;
					if (annotation != null && annotation.Value != null && annotation.Value is string && !string.IsNullOrEmpty((string)annotation.Value))
					{
						string commandText = string.Format("SELECT schema_id FROM sys.schemas WHERE name = '{0}'", annotation.Value);
						using (DbCommand dbCommand = _connection.DbConnection.CreateCommand())
						{
							dbCommand.CommandText = commandText;
							await _connection.OpenAsync();
							object result = await dbCommand.ExecuteScalarAsync();
							_connection.Close();
							if (result != null && result is int && (int)result > 0)
								return true;
							else
								return false;
						}
					}
					else
						return true;
				}, cancellationToken);

		/// <summary>
		///     This API supports the Entity Framework Core infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public override void Delete()
		{
			throw new NotImplementedException(); // TO DO or NOT TO DO?
		}

	}
}
