// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the backing field for a property is changed.
    /// </summary>
    public interface IPropertyFieldChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the backing field for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="newFieldInfo"> The new field. </param>
        /// <param name="oldFieldInfo"> The old field. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo,
            IConventionContext<FieldInfo> context);
    }
}
