// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public abstract class XGJsonTypeMappingSourcePlugin
        : IRelationalTypeMappingSourcePlugin
    {
        [NotNull]
        public virtual IXGOptions Options { get; }

        protected XGJsonTypeMappingSourcePlugin(
            [NotNull] IXGOptions options)
        {
            Options = options;
        }

        public virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;

            if (clrType == typeof(XGJsonString))
            {
                clrType = typeof(string);
                storeTypeName = "json";
            }

            if (storeTypeName != null)
            {
                clrType ??= typeof(string);
                return storeTypeName.Equals("json", StringComparison.OrdinalIgnoreCase)
                    ? (RelationalTypeMapping)Activator.CreateInstance(
                        XGJsonTypeMappingType.MakeGenericType(clrType),
                        storeTypeName,
                        GetValueConverter(clrType),
                        GetValueComparer(clrType),
                        Options.NoBackslashEscapes,
                        Options.ReplaceLineBreaksWithCharFunction)
                    : null;
            }

            return FindDomMapping(mappingInfo);
        }

        protected abstract Type XGJsonTypeMappingType { get; }
        protected abstract RelationalTypeMapping FindDomMapping(RelationalTypeMappingInfo mappingInfo);
        protected abstract ValueConverter GetValueConverter(Type clrType);
        protected abstract ValueComparer GetValueComparer(Type clrType);
    }
}
