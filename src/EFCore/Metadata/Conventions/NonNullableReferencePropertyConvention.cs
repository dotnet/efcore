// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the properties of non-nullable types as required.
    /// </summary>
    public class NonNullableReferencePropertyConvention : NonNullableConventionBase,
        IPropertyAddedConvention,
        IPropertyFieldChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NonNullableReferencePropertyConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NonNullableReferencePropertyConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        private void Process(IConventionPropertyBuilder propertyBuilder)
        {
            // If the model is spread across multiple assemblies, it may contain different NullableAttribute types as
            // the compiler synthesizes them for each assembly.
            if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is MemberInfo memberInfo
                && IsNonNullableReferenceType(propertyBuilder.ModelBuilder, memberInfo))
            {
                propertyBuilder.IsRequired(true);
            }
        }

        /// <summary>
        ///     Called after a property is added to the entity type.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
        {
            Process(propertyBuilder);
        }

        /// <summary>
        ///     Called after the backing field for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="newFieldInfo"> The new field. </param>
        /// <param name="oldFieldInfo"> The old field. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo newFieldInfo,
            FieldInfo oldFieldInfo,
            IConventionContext<FieldInfo> context)
        {
            Process(propertyBuilder);
        }
    }
}
