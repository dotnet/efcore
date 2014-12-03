// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Annotation : IAnnotation
    {
        private readonly string _name;
        private readonly string _value;

        public Annotation([NotNull] string name, [NotNull] string value)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(value, "value");

            _name = name;
            _value = value;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string Value
        {
            get { return _value; }
        }
    }
}
