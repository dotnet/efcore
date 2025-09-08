// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.XuGu.ValueGeneration.Internal
{
    public class XGValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly IXGOptions _options;

        public XGValueGeneratorSelector(
            [NotNull] ValueGeneratorSelectorDependencies dependencies,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ValueGenerator FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        {
            var ret = clrType == typeof(Guid)
                ? property.ValueGenerated == ValueGenerated.Never
                  || property.GetDefaultValueSql() != null
                    ? new TemporaryGuidValueGenerator()
                    : new XGSequentialGuidValueGenerator(_options)
                : base.FindForType(property, typeBase, clrType);
            return ret;
        }
    }
}
