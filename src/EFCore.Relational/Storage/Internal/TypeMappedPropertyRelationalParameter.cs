// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TypeMappedPropertyRelationalParameter : TypeMappedRelationalParameter
    {
        private readonly IClrPropertyGetter _clrPropertyGetter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TypeMappedPropertyRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] RelationalTypeMapping relationalTypeMapping,
            [NotNull] IProperty property)
            : base(invariantName, name, relationalTypeMapping, property.IsNullable)
        {
            _clrPropertyGetter = property.GetGetter();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddDbParameter(DbCommand command, object value)
        {
            Debug.Assert(value != null);

            base.AddDbParameter(command, _clrPropertyGetter.GetClrValue(value));
        }
    }
}
