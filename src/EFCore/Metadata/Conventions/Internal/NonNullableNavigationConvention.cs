// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NonNullableNavigationConvention : INavigationAddedConvention
    {
        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
        private Type _nullableAttrType;
        private FieldInfo _nullableFlagsFieldInfo;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NonNullableNavigationConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder,
            Navigation navigation)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            if (!IsNonNullable(navigation))
            {
                return relationshipBuilder;
            }

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    if (IsNonNullable(inverse))
                    {
                        Logger.NonNullableReferenceOnBothNavigations(navigation, inverse);
                        return relationshipBuilder;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    return relationshipBuilder;
                }

                var newRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType,
                    ConfigurationSource.Convention);

                if (newRelationshipBuilder == null)
                {
                    return relationshipBuilder;
                }

                Logger.NonNullableOnDependent(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            return relationshipBuilder.IsRequired(true, ConfigurationSource.Convention) ?? relationshipBuilder;
        }

        private bool IsNonNullable(Navigation navigation)
            => navigation.DeclaringEntityType.HasClrType()
               && navigation.DeclaringEntityType.GetRuntimeProperties().Find(navigation.Name) is PropertyInfo propertyInfo
               && IsNonNullable(propertyInfo);

        private bool IsNonNullable(MemberInfo memberInfo)
        {
            if (Attribute.GetCustomAttributes(memberInfo, true) is Attribute[] attributes
                && attributes.FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName) is Attribute attribute)
            {
                if (attribute.GetType() != _nullableAttrType)
                {
                    _nullableFlagsFieldInfo = attribute.GetType().GetField("NullableFlags");
                    _nullableAttrType = attribute.GetType();
                }

                // For the interpretation of NullableFlags, see
                // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-reference-types.md#annotations
                if (_nullableFlagsFieldInfo?.GetValue(attribute) is byte[] flags
                    && flags.Length >= 0
                    && flags[0] == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
