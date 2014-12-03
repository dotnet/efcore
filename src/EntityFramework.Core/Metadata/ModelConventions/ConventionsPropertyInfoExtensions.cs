// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    internal static class ConventionsPropertyInfoExtensions
    {
        public static bool IsCandidateProperty(this PropertyInfo propertyInfo)
        {
            return !propertyInfo.IsStatic()
                   && propertyInfo.GetIndexParameters().Length == 0
                   && propertyInfo.CanRead
                   && propertyInfo.CanWrite;
        }

        public static bool IsCandidatePrimitiveProperty(this PropertyInfo propertyInfo)
        {
            if (!IsCandidateProperty(propertyInfo))
            {
                return false;
            }

            return propertyInfo.PropertyType.IsPrimitive();
        }
    }
}
