// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal
{
    public interface IXGOptions : ISingletonOptions
    {
        XGConnectionSettings ConnectionSettings { get; }

        /// <remarks>
        /// If null, there might still be a `DbDataSource` in the ApplicationServiceProvider.
        /// </remarks>>
        DbDataSource DataSource { get; }

        ServerVersion ServerVersion { get; }
        CharSet DefaultCharSet { get; }
        CharSet NationalCharSet { get; }
        string DefaultGuidCollation { get; }
        bool NoBackslashEscapes { get; }
        bool ReplaceLineBreaksWithCharFunction { get; }
        XGDefaultDataTypeMappings DefaultDataTypeMappings { get; }
        XGSchemaNameTranslator SchemaNameTranslator { get; }
        bool IndexOptimizedBooleanColumns { get; }
        XGJsonChangeTrackingOptions JsonChangeTrackingOptions { get; }
        bool LimitKeyedOrIndexedStringColumnLength { get; }
        bool StringComparisonTranslations { get; }
        bool PrimitiveCollectionsSupport { get; }
    }
}
