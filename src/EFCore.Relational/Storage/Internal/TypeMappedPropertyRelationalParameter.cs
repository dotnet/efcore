// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TypeMappedPropertyRelationalParameter : TypeMappedRelationalParameter
    {
        private readonly IClrPropertyGetter _clrPropertyGetter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddDbParameter(DbCommand command, object value)
        {
            Debug.Assert(value != null);

            base.AddDbParameter(command, _clrPropertyGetter.GetClrValue(value));
        }
    }
}
