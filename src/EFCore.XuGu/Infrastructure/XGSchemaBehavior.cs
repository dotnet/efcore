// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.XuGu.Infrastructure
{
    public delegate string XGSchemaNameTranslator(string schemaName, string objectName);

    public enum XGSchemaBehavior
    {
        /// <summary>
        /// Throw an exception if a schema is being used. Any specified translator delegate will be ignored.
        /// This is the default.
        /// </summary>
        Throw,

        /// <summary>
        /// Silently ignore any schema definitions. Any specified translator delegate will be ignored.
        /// </summary>
        Ignore,

        /// <summary>
        /// Use the specified translator delegate to translate from an input schema and object name to
        /// an output object name whenever a schema is being used.
        /// </summary>
        Translate,
    }
}
