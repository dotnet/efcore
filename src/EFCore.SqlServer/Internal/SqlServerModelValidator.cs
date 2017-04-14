// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class SqlServerModelValidator : RelationalModelValidator
    {
        public SqlServerModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            EnsureNoDefaultDecimalMapping(model);
            EnsureNoByteIdentityMapping(model);
            EnsureNoNonKeyValueGeneration(model);
        }

        protected virtual void EnsureNoDefaultDecimalMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => p.ClrType.UnwrapNullableType() == typeof(decimal)
                         && p.SqlServer().ColumnType == null))
            {
                Dependencies.Logger.DecimalTypeDefaultWarning(property);
            }
        }

        protected virtual void EnsureNoByteIdentityMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => p.ClrType.UnwrapNullableType() == typeof(byte)
                         && p.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn))
            {
                Dependencies.Logger.ByteIdentityColumnWarning(property);
            }
        }

        protected virtual void EnsureNoNonKeyValueGeneration([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p =>
                        (((SqlServerPropertyAnnotations)p.SqlServer()).GetSqlServerValueGenerationStrategy(fallbackToModel: false) == SqlServerValueGenerationStrategy.SequenceHiLo
                         || ((SqlServerPropertyAnnotations)p.SqlServer()).GetSqlServerValueGenerationStrategy(fallbackToModel: false) == SqlServerValueGenerationStrategy.IdentityColumn)
                        && !p.IsKey()))
            {
                ShowError(SqlServerStrings.NonKeyValueGeneration(property.Name, property.DeclaringEntityType.DisplayName()));
            }
        }
    }
}
