// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertyChangedInterceptor : PropertyChangeInterceptorBase, IInterceptor
    {
        private static readonly Type _notifyChangedInterface = typeof(INotifyPropertyChanged);

        private readonly bool _checkEquality;
        private PropertyChangedEventHandler _handler;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PropertyChangedInterceptor(
            [NotNull] IEntityType entityType,
            bool checkEquality)
            : base(entityType)
        {
            _checkEquality = checkEquality;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;

            if (invocation.Method.DeclaringType == _notifyChangedInterface)
            {
                if (methodName == $"add_{nameof(INotifyPropertyChanged.PropertyChanged)}")
                {
                    _handler = (PropertyChangedEventHandler)Delegate.Combine(
                        _handler, (Delegate)invocation.Arguments[0]);
                }
                else if (methodName == $"remove_{nameof(INotifyPropertyChanged.PropertyChanged)}")
                {
                    _handler = (PropertyChangedEventHandler)Delegate.Remove(
                        _handler, (Delegate)invocation.Arguments[0]);
                }
            }
            else if (methodName.StartsWith("set_", StringComparison.Ordinal))
            {
                var propertyName = FindPropertyName(invocation);

                var property = EntityType.FindProperty(propertyName);
                if (property != null)
                {
                    HandleChanged(invocation, property, GetValueComparer(property));
                }
                else
                {
                    var navigation = EntityType.FindNavigation(propertyName)
                        ?? (INavigationBase)EntityType.FindSkipNavigation(propertyName);

                    if (navigation != null)
                    {
                        HandleChanged(invocation, navigation, LegacyReferenceEqualityComparer.Instance);
                    }
                    else
                    {
                        invocation.Proceed();
                    }
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        private void HandleChanged(IInvocation invocation, IPropertyBase property, IEqualityComparer comparer)
        {
            var newValue = invocation.Arguments[^1];

            if (_checkEquality)
            {
                var oldValue = property.GetGetter().GetClrValue(invocation.Proxy);

                invocation.Proceed();

                if (!(comparer?.Equals(oldValue, newValue) ?? Equals(oldValue, newValue)))
                {
                    NotifyPropertyChanged(property.Name, invocation.Proxy);
                }
                else
                {
                    invocation.Proceed();
                }
            }
            else
            {
                invocation.Proceed();
                NotifyPropertyChanged(property.Name, invocation.Proxy);
            }
        }

        private void NotifyPropertyChanged(string propertyName, object proxy)
            => _handler?.Invoke(proxy, new PropertyChangedEventArgs(propertyName));
    }
}
