// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalModelAnnotations : ReadOnlyRelationalModelAnnotations
    {
        public RelationalModelAnnotations([NotNull] Model model)
            : base(model)
        {
        }

        public virtual Sequence GetOrAddSequence([CanBeNull] string name, [CanBeNull] string schema = null) 
            => new Sequence(
                (Model)Model, 
                RelationalAnnotationNames.Prefix, 
                Check.NotEmpty(name, nameof(name)), 
                Check.NullButNotEmpty(schema, nameof(schema)));

    }
}
