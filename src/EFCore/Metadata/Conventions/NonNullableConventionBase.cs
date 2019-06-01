// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A base type for conventions that configure model aspects based on whether the member type
    ///     is a non-nullable reference type.
    /// </summary>
    public abstract class NonNullableConventionBase
    {
        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
        private Type _nullableAttrType;
        private FieldInfo _nullableFlagsFieldInfo;

        /// <summary>
        ///     Creates a new instance of <see cref="NonNullableConventionBase" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected NonNullableConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Returns a value indicating whether the member type is a non-nullable reference type.
        /// </summary>
        /// <param name="memberInfo"> The member info. </param>
        /// <returns> <c>true</c> if the member type is a non-nullable reference type. </returns>
        protected virtual bool IsNonNullable([NotNull] MemberInfo memberInfo)
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
