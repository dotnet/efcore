// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.ValueComparison.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal
{
    public class XGJsonNewtonsoftTypeMappingSourcePlugin : XGJsonTypeMappingSourcePlugin
    {
        private static readonly Lazy<XGJsonNewtonsoftJTokenValueConverter> _jTokenValueConverter = new Lazy<XGJsonNewtonsoftJTokenValueConverter>();
        private static readonly Lazy<XGJsonNewtonsoftStringValueConverter> _jsonStringValueConverter = new Lazy<XGJsonNewtonsoftStringValueConverter>();

        public XGJsonNewtonsoftTypeMappingSourcePlugin(
            [NotNull] IXGOptions options)
            : base(options)
        {
        }

        protected override Type XGJsonTypeMappingType => typeof(XGJsonNewtonsoftTypeMapping<>);

        protected override RelationalTypeMapping FindDomMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;

            if (typeof(JToken).IsAssignableFrom(clrType))
            {
                return (RelationalTypeMapping)Activator.CreateInstance(
                    XGJsonTypeMappingType.MakeGenericType(clrType),
                    "json",
                    GetValueConverter(clrType),
                    GetValueComparer(clrType),
                    Options.NoBackslashEscapes,
                    Options.ReplaceLineBreaksWithCharFunction);
            }

            return null;
        }

        protected override ValueConverter GetValueConverter(Type clrType)
        {
            if (typeof(JToken).IsAssignableFrom(clrType))
            {
                return _jTokenValueConverter.Value;
            }

            if (clrType == typeof(string))
            {
                return _jsonStringValueConverter.Value;
            }

            return (ValueConverter)Activator.CreateInstance(
                typeof(XGJsonNewtonsoftPocoValueConverter<>).MakeGenericType(clrType));
        }

        protected override ValueComparer GetValueComparer(Type clrType)
            => XGJsonNewtonsoftValueComparer.Create(clrType, Options.JsonChangeTrackingOptions);
    }
}
