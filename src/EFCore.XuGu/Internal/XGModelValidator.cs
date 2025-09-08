// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGu.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class XGModelValidator : RelationalModelValidator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        protected override void ValidateJsonEntities(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.IsMappedToJson())
                {
                    throw new InvalidOperationException(XGStrings.Ef7CoreJsonMappingNotSupported);
                }
            }
        }

        /// <inheritdoc />
        protected override void ValidateStoredProcedures(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateStoredProcedures(model, logger);

            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.GetDeleteStoredProcedure() is { } deleteStoredProcedure)
                {
                    ValidateSproc(deleteStoredProcedure, logger);
                }

                if (entityType.GetInsertStoredProcedure() is { } insertStoredProcedure)
                {
                    ValidateSproc(insertStoredProcedure, logger);
                }

                if (entityType.GetUpdateStoredProcedure() is { } updateStoredProcedure)
                {
                    ValidateSproc(updateStoredProcedure, logger);
                }
            }

            static void ValidateSproc(IStoredProcedure sproc, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            {
                var entityType = sproc.EntityType;
                var storeObjectIdentifier = sproc.GetStoreIdentifier();

                if (sproc.ResultColumns.Any())
                {
                    throw new InvalidOperationException(XGStrings.StoredProcedureResultColumnsNotSupported(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName()));
                }

                if (sproc.IsRowsAffectedReturned)
                {
                    throw new InvalidOperationException(XGStrings.StoredProcedureReturnValueNotSupported(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName()));
                }
            }
        }
    }
}
