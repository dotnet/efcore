// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledAnnotationsBase
    {
        public static readonly Annotation[] EmptyAnnotations = new Annotation[0];

        public readonly Annotation[] Annotations;

        public CompiledAnnotationsBase([NotNull] string[] names, [NotNull] string[] values)
        {
            Check.NotNull(names, "names");
            Check.NotNull(values, "values");

            Annotations = names.Zip(values, (n, v) => new Annotation(n, v)).ToArray();
        }
    }
}
