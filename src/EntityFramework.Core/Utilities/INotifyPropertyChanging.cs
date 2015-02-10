// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

#if !NET45 && !ASPNET50 && !ASPNETCORE50
// TODO: Remove this file when VS can build against ".NET Portable 5.0"

namespace Microsoft.Data.Entity.Utilities
{
    internal interface INotifyPropertyChanging
    {
        event PropertyChangingEventHandler PropertyChanging;
    }

    internal delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);

    internal class PropertyChangingEventArgs : EventArgs
    {
        private readonly string _propertyName;

        public PropertyChangingEventArgs([CanBeNull] string propertyName)
        {
            _propertyName = propertyName;
        }

        public virtual string PropertyName
        {
            get { return _propertyName; }
        }
    }
}

#endif
