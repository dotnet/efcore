// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class FunctionMapping : TableMappingBase, IFunctionMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public FunctionMapping(
            [NotNull] IEntityType entityType,
            [NotNull] StoreFunction storeFunction,
            [NotNull] DbFunction dbFunction,
            bool includesDerivedTypes)
            : base(entityType, storeFunction, includesDerivedTypes)
        {
            DbFunction = dbFunction;
        }

        /// <inheritdoc />
        public virtual bool IsDefaultFunctionMapping { get; set; }

        /// <inheritdoc />
        public virtual IStoreFunction StoreFunction
            => (IStoreFunction)base.Table;

        /// <inheritdoc />
        public virtual IDbFunction DbFunction { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IEnumerable<IFunctionColumnMapping> IFunctionMapping.ColumnMappings
        {
            [DebuggerStepThrough]
            get => ColumnMappings.Cast<IFunctionColumnMapping>();
        }
    }
}
