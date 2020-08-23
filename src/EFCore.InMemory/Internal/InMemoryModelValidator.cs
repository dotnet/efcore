// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InMemoryModelValidator : ModelValidator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryModelValidator([NotNull] ModelValidatorDependencies dependencies)
            : base(dependencies)
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

            ValidateDefiningQuery(model, logger);
        }

        /// <summary>
        ///     Validates the configuration of defining queries in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateDefiningQuery(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.GetInMemoryQuery() != null)
                {
                    if (entityType.BaseType != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.DerivedTypeDefiningQuery(entityType.DisplayName(), entityType.BaseType.DisplayName()));
                    }
                }
            }
        }
    }
}
