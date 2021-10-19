// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
    public class SqliteModelValidator : RelationalModelValidator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteModelValidator(
            ModelValidatorDependencies dependencies,
            RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.Validate(model, logger);

            ValidateNoSchemas(model, logger);
            ValidateNoSequences(model, logger);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateNoSchemas(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes().Where(e => e.GetSchema() != null))
            {
                logger.SchemaConfiguredWarning(entityType, entityType.GetSchema()!);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateNoSequences(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var sequence in model.GetSequences())
            {
                logger.SequenceConfiguredWarning(sequence);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ValidateCompatible(
            IProperty property,
            IProperty duplicateProperty,
            string columnName,
            in StoreObjectIdentifier storeObject,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

            var propertySrid = property.GetSrid(storeObject);
            var duplicatePropertySrid = duplicateProperty.GetSrid(storeObject);
            if (propertySrid != duplicatePropertySrid)
            {
                throw new InvalidOperationException(
                    SqliteStrings.DuplicateColumnNameSridMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }
        }
    }
}
