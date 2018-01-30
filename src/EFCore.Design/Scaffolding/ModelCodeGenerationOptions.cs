// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents the options to use while generating code for a model.
    /// </summary>
    public class ModelCodeGenerationOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to use data annotations.
        /// </summary>
        /// <value> A value indicating whether to use data annotations. </value>
        public virtual bool UseDataAnnotations { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to suppress the connection string sensitive information warning.
        /// </summary>
        /// <value> A value indicating whether to suppress the connection string sensitive information warning. </value>
        public virtual bool SuppressConnectionStringWarning { get; set; }
    }
}
