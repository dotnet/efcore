// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A base type for conventions that perform configuration based on an attribute applied to a property.
    /// </summary>
    /// <typeparam name="TAttribute"> The attribute type to look for. </typeparam>
    public abstract class PropertyAttributeConventionBase<TAttribute> : IPropertyAddedConvention, IPropertyFieldChangedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance of <see cref="PropertyAttributeConventionBase{TAttribute}" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected PropertyAttributeConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a property is added to the entity type.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var memberInfo = propertyBuilder.Metadata.GetIdentifyingMemberInfo();
            if (memberInfo == null)
            {
                return;
            }

            Process(propertyBuilder, memberInfo, (IReadableConventionContext)context);
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
            if (newFieldInfo != null
                && propertyBuilder.Metadata.PropertyInfo == null)
            {
                Process(propertyBuilder, newFieldInfo, (IReadableConventionContext)context);
            }
        }

        private void Process(IConventionPropertyBuilder propertyBuilder, MemberInfo memberInfo, IReadableConventionContext context)
        {
            if (!Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit: true))
            {
                return;
            }

            var attributes = memberInfo.GetCustomAttributes<TAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                ProcessPropertyAdded(propertyBuilder, attribute, memberInfo, context);
                if (context.ShouldStopProcessing())
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="clrMember"> The member that has the attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected abstract void ProcessPropertyAdded(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [NotNull] TAttribute attribute,
            [NotNull] MemberInfo clrMember,
            [NotNull] IConventionContext context);
    }
}
