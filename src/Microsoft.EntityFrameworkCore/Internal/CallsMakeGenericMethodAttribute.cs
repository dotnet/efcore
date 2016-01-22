// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     Method uses reflection to create a generic method. e.g. MethodInfo.MakeGenericMethod().
    ///     Runtime-directive scaffolding uses this attribute to compute the possible runtime arguments that may be passed to MakeGenericMethod.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CallsMakeGenericMethodAttribute : Attribute
    {
        public CallsMakeGenericMethodAttribute([NotNull] string methodName, [NotNull] params Type[] typeArguments)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotEmpty(typeArguments, nameof(typeArguments));

            MethodName = methodName;
            TypeArguments = typeArguments;
        }

        public CallsMakeGenericMethodAttribute([NotNull] Type targetType, [NotNull] string methodName, [NotNull] params Type[] typeArguments)
            : this(methodName, typeArguments)
        {
            Check.NotNull(targetType, nameof(targetType));

            TargetType = targetType;
        }

        public CallsMakeGenericMethodAttribute()
        {
        }

        /// <summary>
        ///     Type arguments (or category of type arguments <see cref="TypeArgumentCategory" />) used
        ///     in the MakeGenericMethod
        /// </summary>
        public virtual Type[] TypeArguments { get; [param: NotNull] set; }

        /// <summary>
        ///     Name of the method created
        /// </summary>
        public virtual string MethodName { get; [param: NotNull] set; }

        /// <summary>
        ///     Type which contains the method to be created
        /// </summary>
        public virtual Type TargetType { get; [param: NotNull] set; }

        public virtual MethodInfo FindMethodInfo([NotNull] MethodInfo declaringMethod)
            => string.IsNullOrEmpty(MethodName)
                ? declaringMethod
                : (TargetType ?? declaringMethod.DeclaringType)
                    .GetTypeInfo()
                    .GetDeclaredMethods(MethodName)
                    .FirstOrDefault(m => m.GetGenericArguments().Length == TypeArguments.Length);
    }
}
