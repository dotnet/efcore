// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class CommandBuilder
    {
        private readonly SelectExpression _selectExpression;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        public CommandBuilder(
            [NotNull] SelectExpression selectExpression,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(selectExpression, "selectExpression");
            Check.NotNull(relationalQueryCompilationContext, "relationalQueryCompilationContext");

            _selectExpression = selectExpression;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        public virtual DbCommand Build([NotNull] RelationalConnection connection)
        {
            Check.NotNull(connection, "connection");

            // TODO: Cache command...

            var command = connection.DbConnection.CreateCommand();
            if (connection.Transaction != null)
            {
                command.Transaction = connection.Transaction.DbTransaction;
            }

            var sqlGenerator = _relationalQueryCompilationContext.CreateSqlQueryGenerator();

            command.CommandText = sqlGenerator.GenerateSql(_selectExpression);

            foreach (var commandParameter in sqlGenerator.Parameters)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = commandParameter.Name;
                parameter.Value = commandParameter.Value;

                // TODO: Parameter facets

                command.Parameters.Add(parameter);
            }

            return command;
        }
    }
}
