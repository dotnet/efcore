// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueComparison.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.Internal
{
    public class XGJsonMicrosoftTypeMappingSourcePlugin : XGJsonTypeMappingSourcePlugin
    {
        private static readonly Lazy<XGJsonMicrosoftJsonDocumentValueConverter> _jsonDocumentValueConverter = new Lazy<XGJsonMicrosoftJsonDocumentValueConverter>();
        private static readonly Lazy<XGJsonMicrosoftJsonElementValueConverter> _jsonElementValueConverter = new Lazy<XGJsonMicrosoftJsonElementValueConverter>();
        private static readonly Lazy<XGJsonMicrosoftStringValueConverter> _jsonStringValueConverter = new Lazy<XGJsonMicrosoftStringValueConverter>();

        public XGJsonMicrosoftTypeMappingSourcePlugin(
            [NotNull] IXGOptions options)
            : base(options)
        {
        }

        protected override Type XGJsonTypeMappingType => typeof(XGJsonMicrosoftTypeMapping<>);

        protected override RelationalTypeMapping FindDomMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;

            if (clrType == typeof(JsonDocument) ||
                clrType == typeof(JsonElement))
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
            if (clrType == typeof(JsonDocument))
            {
                return _jsonDocumentValueConverter.Value;
            }

            if (clrType == typeof(JsonElement))
            {
                return _jsonElementValueConverter.Value;
            }

            if (clrType == typeof(string))
            {
                return _jsonStringValueConverter.Value;
            }

            return (ValueConverter)Activator.CreateInstance(
                typeof(XGJsonMicrosoftPocoValueConverter<>).MakeGenericType(clrType));
        }

        protected override ValueComparer GetValueComparer(Type clrType)
            => XGJsonMicrosoftValueComparer.Create(clrType, Options.JsonChangeTrackingOptions);
    }
}
