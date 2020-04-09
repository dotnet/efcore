// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public abstract class MigrationSqlGeneratorTestBase
    {
        protected static string EOL => Environment.NewLine;

        protected virtual string Sql { get; set; }

        [ConditionalFact]
        public virtual void AddColumnOperation_without_column_type()
            => Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Alias",
                    ClrType = typeof(string)
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity<Person>().Property<string>("Name").IsUnicode(false),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = true,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_fixed_length_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true,
                    IsFixedLength = true,
                    MaxLength = 100
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity<Person>().Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 32,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_precision_and_scale_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity<Person>().Property<decimal>("Pi").HasPrecision(30, 17),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Pi",
                    ClrType = typeof(decimal),
                    Precision = 15,
                    Scale = 10
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_precision_and_scale_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Pi",
                    ClrType = typeof(decimal),
                    Precision = 20,
                    Scale = 7
                });

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_without_principal_columns()
            => Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "SpouseId" },
                    PrincipalTable = "People"
                });

        [ConditionalFact]
        public virtual void AlterColumnOperation_without_column_type()
            => Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LuckyNumber",
                    ClrType = typeof(int)
                });

        [ConditionalFact]
        public virtual void RenameTableOperation_legacy()
            => Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person"
                });

        [ConditionalFact]
        public virtual void RenameTableOperation()
            => Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person",
                    NewSchema = "dbo"
                });

        [ConditionalFact]
        public virtual void SqlOperation()
            => Generate(
                new SqlOperation { Sql = "-- I <3 DDL" });

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void DefaultValue_with_line_breaks(bool isUnicode)
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "TestLineBreaks",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "TestDefaultValue",
                            Table = "Test",
                            ClrType = typeof(string),
                            DefaultValue = "\r\nVarious Line\rBreaks\n",
                            IsUnicode = isUnicode
                        }
                    }
                });
        }

        protected TestHelpers TestHelpers { get; }

        protected MigrationSqlGeneratorTestBase(TestHelpers testHelpers)
        {
            TestHelpers = testHelpers;
        }

        protected virtual void Generate(params MigrationOperation[] operation)
            => Generate(_ => { }, operation);

        protected virtual void Generate(Action<ModelBuilder> buildAction, params MigrationOperation[] operation)
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
            buildAction(modelBuilder);

            var services = TestHelpers.CreateContextServices();

            IModel model = modelBuilder.Model;
            var conventionSet = services.GetRequiredService<IConventionSetBuilder>().CreateConventionSet();
            var relationalModelConvention = conventionSet.ModelFinalizedConventions.OfType<RelationalModelConvention>().First();
            model = relationalModelConvention.ProcessModelFinalized((IConventionModel)model);
            model = ((IMutableModel)model).FinalizeModel();

            var batch = services.GetRequiredService<IMigrationsSqlGenerator>().Generate(operation, modelBuilder.Model);

            Sql = string.Join(
                "GO" + EOL + EOL,
                batch.Select(b => b.CommandText));
        }

        protected void AssertSql(string expected)
            => Assert.Equal(expected, Sql, ignoreLineEndingDifferences: true);

        protected class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Pi { get; set; }
        }
    }
}
