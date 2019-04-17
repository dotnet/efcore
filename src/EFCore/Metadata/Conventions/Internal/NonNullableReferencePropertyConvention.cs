// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NonNullableReferencePropertyConvention : IPropertyAddedConvention, IPropertyFieldChangedConvention
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
        public NonNullableReferencePropertyConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
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
        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            // If the model is spread across multiple assemblies, it may contain different NullableAttribute types as
            // the compiler synthesizes them for each assembly.
            if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is MemberInfo memberInfo
                && IsNonNullable(memberInfo))
            {
                propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
            }

            return propertyBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
        {
            Apply(propertyBuilder);
            return true;
        }

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
