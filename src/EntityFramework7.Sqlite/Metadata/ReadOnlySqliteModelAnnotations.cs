// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqliteModelAnnotations : ReadOnlyRelationalModelAnnotations, ISqliteModelAnnotations
    {
        public ReadOnlySqliteModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        public override Sequence TryGetSequence(string name, string schema = null) => null;
        public override IReadOnlyList<Sequence> Sequences => new Sequence[0];
    }
}
