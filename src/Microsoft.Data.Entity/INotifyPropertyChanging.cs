// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET45
// TODO: This should be shipped in some other assembly/NuGet package with type-forwarding/unification for full .NET

namespace System.ComponentModel
{
    public interface INotifyPropertyChanging
    {
        event PropertyChangingEventHandler PropertyChanging;
    }

    public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);

    public class PropertyChangingEventArgs : EventArgs
    {
        private readonly string _propertyName;

        public PropertyChangingEventArgs(string propertyName)
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
