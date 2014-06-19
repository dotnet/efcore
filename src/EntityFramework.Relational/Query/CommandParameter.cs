// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class CommandParameter
    {
        private readonly string _name;
        private readonly object _value;

        public CommandParameter([NotNull] string name, [NotNull] object value)
        {
            Check.NotNull(name, "name");
            Check.NotNull(value, "value");

            _name = name;
            _value = value;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual object Value
        {
            get { return _value; }
        }
    }
}
