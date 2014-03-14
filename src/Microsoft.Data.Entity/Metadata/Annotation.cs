// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Annotation : NamedMetadataBase, IAnnotation
    {
        private readonly string _value;

        public Annotation([NotNull] string name, [NotNull] string value)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotEmpty(value, "value");

            _value = value;
        }

        public virtual string Value
        {
            get { return _value; }
        }
    }
}
