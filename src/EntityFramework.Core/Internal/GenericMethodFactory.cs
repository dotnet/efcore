// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    /// <summary>
    ///     Method uses reflection to create a generic method. e.g. MethodInfo.MakeGenericMethod().
    ///     Runtime-directive scaffolding uses this attribute to compute the possible runtime arguments that may be passed to MakeGenericMethod.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class GenericMethodFactory : Attribute
    {
        public GenericMethodFactory([NotNull] string methodName, [NotNull] params Type[] typeArguments)
            : this(typeArguments)
        {
            Check.NotEmpty(methodName, nameof(methodName));

            MethodName = methodName;
        }

        public GenericMethodFactory([NotNull] params Type[] typeArguments)
        {
            Check.NotEmpty(typeArguments, nameof(typeArguments));

            TypeArguments = typeArguments;
        }

        public GenericMethodFactory()
        {
        }

        /// <summary>
        ///     Type arguments (or category of type arguments <see cref="TypeArgumentCategory"/>) used 
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
    }
}
