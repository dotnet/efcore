// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Relational.Migrations.Utilities
{
    internal static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetNonIndexerProperties(this Type type)
        {
            return type.GetRuntimeProperties().Where(p => p.IsPublic() && !p.GetIndexParameters().Any());
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, params Type[] parameterTypes)
        {
            return type.GetTypeInfo().DeclaredConstructors.SingleOrDefault(
                c => !c.IsStatic && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        public static ConstructorInfo GetPublicConstructor(this Type type, params Type[] parameterTypes)
        {
            var constructor = type.GetDeclaredConstructor(parameterTypes);

            return constructor != null && constructor.IsPublic ? constructor : null;
        }
    }
}
