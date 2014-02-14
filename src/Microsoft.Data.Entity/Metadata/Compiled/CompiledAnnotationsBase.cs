// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledAnnotationsBase
    {
        public readonly Annotation[] Annotations;

        public CompiledAnnotationsBase([NotNull] string[] names, [NotNull] string[] values)
        {
            Annotations = names.Zip(values, (n, v) => new Annotation(n, v)).ToArray();
        }
    }
}
