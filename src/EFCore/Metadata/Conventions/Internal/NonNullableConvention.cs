// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class NonNullableConvention
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
        protected virtual bool IsNonNullable(MemberInfo memberInfo)
        {
            // For C# 8.0 nullable types, the C# currently synthesizes a NullableAttribute that expresses nullability into assemblies
            // it produces. If the model is spread across more than one assembly, there will be multiple versions of this attribute,
            // so look for it by name, caching to avoid reflection on every check.
            // Note that this may change - if https://github.com/dotnet/corefx/issues/36222 is done we can remove all of this.
            if (!(Attribute.GetCustomAttributes(memberInfo, true)
                    .FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName)
                is { } attribute))
            {
                return false;
            }

            var attributeType = attribute.GetType();
            if (attributeType != _nullableAttrType)
            {
                _nullableFlagsFieldInfo = attributeType.GetField("NullableFlags");
                _nullableAttrType = attributeType;
            }

            // For the interpretation of NullableFlags, see
            // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-reference-types.md#annotations
            if (_nullableFlagsFieldInfo?.GetValue(attribute) is byte[] flags
                && flags.FirstOrDefault() == 1)
            {
                return true;
            }

            return false;
        }
    }
}
