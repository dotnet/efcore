// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Identity
{
    public class GeneratedValue
    {
        private readonly object _value;
        private readonly bool _isTemporary;

        public GeneratedValue([CanBeNull] object value, bool isTemporary = false)
        {
            _value = value;
            _isTemporary = isTemporary;
        }

        public virtual object Value
        {
            get { return _value; }
        }

        public virtual bool IsTemporary
        {
            get { return _isTemporary; }
        }
    }
}
