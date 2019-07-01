// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        // For the interpretation of nullability metadata, see
        // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md

        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";
        private Type _nullableAttrType;
        private Type _nullableContextAttrType;
        private FieldInfo _nullableFlagsFieldInfo;
        private FieldInfo _nullableContextFlagFieldInfo;

        private readonly Dictionary<Type, byte?> _typeNullabilityCache;
        private readonly Dictionary<Module, byte?> _moduleNullabilityCache;

        /// <summary>
        ///     Creates a new instance of <see cref="NonNullableConventionBase" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected NonNullableConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;

            _typeNullabilityCache = new Dictionary<Type, byte?>();
            _moduleNullabilityCache = new Dictionary<Module, byte?>();
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        private byte? GetNullabilityContextFlag(Attribute[] attributes)
        {
            if (attributes.FirstOrDefault(a => a.GetType().FullName == NullableContextAttributeFullName) is object attribute)
            {
                var attributeType = attribute.GetType();
                if (attributeType != _nullableContextAttrType)
                {
                    _nullableContextFlagFieldInfo = attributeType.GetField("Flag");
                    _nullableContextAttrType = attributeType;
                }

                if (_nullableContextFlagFieldInfo?.GetValue(attribute) is byte flag)
                {
                    return flag;
                }
            }

            return null;
        }

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

            if (Attribute.GetCustomAttributes(memberInfo, true)
                    .FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName) is { } attribute)
            {
                var attributeType = attribute.GetType();
                if (attributeType != _nullableAttrType)
                {
                    _nullableFlagsFieldInfo = attributeType.GetField("NullableFlags");
                    _nullableAttrType = attributeType;
                }

                if (_nullableFlagsFieldInfo?.GetValue(attribute) is byte[] flags
                    && flags.FirstOrDefault() == 1)
                {
                    return true;
                }
            }

            // No attribute on the member, try to find a NullableContextAttribute on the declaring type
            var type = memberInfo.DeclaringType;
            if (type != null)
            {
                if (!_typeNullabilityCache.TryGetValue(type, out var typeContextFlag))
                {
                    typeContextFlag = _typeNullabilityCache[type] =
                        GetNullabilityContextFlag(Attribute.GetCustomAttributes(type));
                }
                if (typeContextFlag.HasValue)
                {
                    return typeContextFlag.Value == 1;
                }
            }

            // Finally, try at the module level.
            var module = memberInfo.Module;
            if (!_moduleNullabilityCache.TryGetValue(module, out var moduleContextFlag))
            {
                moduleContextFlag = _moduleNullabilityCache[module] =
                    GetNullabilityContextFlag(Attribute.GetCustomAttributes(memberInfo.Module));

                if (type != null)
                {
                    _typeNullabilityCache[type] = moduleContextFlag;
                }
            }
            if (moduleContextFlag.HasValue)
            {
                return moduleContextFlag.Value == 1;
            }

            // No NullableAttribute on the member or NullableContextAttribute on the type or module - we're oblivious.
            return false;
        }
    }
}
