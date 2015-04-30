// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

#if !NET45 && !DNXCORE50
// TODO: Remove this file when VS can build against ".NET Portable 5.0"

namespace Microsoft.Data.Entity
{
    public interface INotifyPropertyChanging
    {
        event PropertyChangingEventHandler PropertyChanging;
    }

    public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);

    public class PropertyChangingEventArgs : EventArgs
    {
        public PropertyChangingEventArgs([CanBeNull] string propertyName)
        {
            PropertyName = propertyName;
        }

        public virtual string PropertyName { get; }
    }
}

#endif
