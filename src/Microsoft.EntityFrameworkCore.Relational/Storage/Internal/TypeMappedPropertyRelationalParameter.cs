// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class TypeMappedPropertyRelationalParameter : TypeMappedRelationalParameter
    {
        private readonly IClrPropertyGetter _clrPropertyGetter;

        public TypeMappedPropertyRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] RelationalTypeMapping relationalTypeMapping,
            [NotNull] IProperty property)
            : base(invariantName, name, relationalTypeMapping, property.IsNullable)
        {
            _clrPropertyGetter = property.GetGetter();
        }

        public override void AddDbParameter(DbCommand command, object value)
        {
            Debug.Assert(value != null);

            base.AddDbParameter(command, _clrPropertyGetter.GetClrValue(value));
        }
    }
}
