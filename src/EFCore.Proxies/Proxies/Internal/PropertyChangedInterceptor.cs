// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertyChangedInterceptor : IInterceptor
    {
        private static readonly Type _notifyChangedInterface = typeof(INotifyPropertyChanged);

        private readonly IEntityType _entityType;
        private readonly bool _checkEquality;
        private PropertyChangedEventHandler _handler;
        private Type _proxyType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PropertyChangedInterceptor(
            [NotNull] IEntityType entityType,
            bool checkEquality)
        {
            _entityType = entityType;
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
            if (_proxyType == null)
            {
                _proxyType = invocation.Proxy.GetType();
            }

            var methodName = invocation.Method.Name;

            if (invocation.Method.DeclaringType.Equals(_notifyChangedInterface))
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
                var propertyName = methodName.Substring(4);

                var property = _entityType.FindProperty(propertyName);
                if (property != null)
                {
                    HandleChanged(invocation, propertyName);
                }
                else
                {
                    var navigation = _entityType.FindNavigation(propertyName);
                    if (navigation != null)
                    {
                        HandleChanged(invocation, propertyName);
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

        private void HandleChanged(IInvocation invocation, string propertyName)
        {
            if (_checkEquality)
            {
                if (_proxyType == null)
                {
                    _proxyType = invocation.Proxy.GetType();
                }

                var property = _proxyType.GetProperty(propertyName);
                if (property != null)
                {
                    var oldValue = property.GetValue(invocation.Proxy);
                    var newValue = invocation.Arguments[^1];

                    invocation.Proceed();

                    if (oldValue != newValue)
                    {
                        NotifyPropertyChanged(propertyName, invocation.Proxy);
                    }
                }
                else
                {
                    invocation.Proceed();
                }
            }
            else
            {
                invocation.Proceed();
                NotifyPropertyChanged(propertyName, invocation.Proxy);
            } 
        }

        private void NotifyPropertyChanged(string propertyName, object proxy)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            _handler?.Invoke(proxy, args);
        }
    }
}
