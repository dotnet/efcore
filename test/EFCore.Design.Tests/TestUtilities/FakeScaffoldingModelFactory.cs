// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class FakeScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        public FakeScaffoldingModelFactory(
            IOperationReporter reporter,
            ICandidateNamingService candidateNamingService,
            IPluralizer pluralizer,
            ICSharpUtilities cSharpUtilities,
            IScaffoldingTypeMapper scaffoldingTypeMapper,
            LoggingDefinitions loggingDefinitions)
            : base(reporter, candidateNamingService, pluralizer, cSharpUtilities, scaffoldingTypeMapper, loggingDefinitions)
        {
        }

        public override IModel Create(DatabaseModel databaseModel, bool useDatabaseNames)
        {
            foreach (var sequence in databaseModel.Sequences)
            {
                sequence.Database = databaseModel;
            }

            foreach (var table in databaseModel.Tables)
            {
                table.Database = databaseModel;

                foreach (var column in table.Columns)
                {
                    column.Table = table;
                }

                if (table.PrimaryKey != null)
                {
                    table.PrimaryKey.Table = table;
                    FixupColumns(table, table.PrimaryKey.Columns);
                }

                foreach (var index in table.Indexes)
                {
                    index.Table = table;
                    FixupColumns(table, index.Columns);
                }

                foreach (var uniqueConstraints in table.UniqueConstraints)
                {
                    uniqueConstraints.Table = table;
                    FixupColumns(table, uniqueConstraints.Columns);
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    foreignKey.Table = table;
                    FixupColumns(table, foreignKey.Columns);

                    FixupColumns(foreignKey.PrincipalTable, foreignKey.PrincipalColumns);
                }
            }

            return base.Create(databaseModel, useDatabaseNames);
        }

        private static void FixupColumns(DatabaseTable table, IList<DatabaseColumn> columns)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                columns[i].Table = table;
            }
        }
    }
}
