// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class SqlServerModelValidator : RelationalModelValidator
    {
        public SqlServerModelValidator(
            [NotNull] ILogger<RelationalModelValidator> loggerFactory,
            [NotNull] IRelationalAnnotationProvider relationalExtensions,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(loggerFactory, relationalExtensions, typeMapper)
        {
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);
            EnsureNoDefaultDecimalMapping(model);
        }

        protected virtual void EnsureNoDefaultDecimalMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes().SelectMany(t => t.GetDeclaredProperties())
                .Where(p => p.ClrType == typeof(decimal) && p.SqlServer().ColumnType == null))
            {
                ShowWarning(SqlServerEventId.DefaultDecimalTypeWarning,
                    SqlServerStrings.DefaultDecimalTypeColumn(property.Name, property.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ShowWarning(SqlServerEventId eventId, [NotNull] string message)
            => Logger.LogWarning(eventId, () => message);
    }
}
