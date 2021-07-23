// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class FunctionColumnMapping : ColumnMappingBase, IFunctionColumnMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public FunctionColumnMapping(
            IProperty property,
            FunctionColumn column,
            FunctionMapping viewMapping)
            : base(property, column, viewMapping)
        {
        }

        /// <inheritdoc />
        public virtual IFunctionMapping FunctionMapping
            => (IFunctionMapping)TableMapping;

        /// <inheritdoc />
        public override RelationalTypeMapping TypeMapping => Property.FindRelationalTypeMapping(
            StoreObjectIdentifier.DbFunction(FunctionMapping.DbFunction.Name))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((IFunctionColumnMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        IFunctionColumn IFunctionColumnMapping.Column
        {
            [DebuggerStepThrough]
            get => (IFunctionColumn)Column;
        }
    }
}
