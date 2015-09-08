// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteModelAnnotations : ReadOnlySqliteModelAnnotations
    {
        public SqliteModelAnnotations([NotNull] Model model)
            : base(model)
        {
        }

        protected new virtual Model Model => (Model)base.Model;
    }
}
