// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertyChangingInterceptor : IInterceptor
    {
        private static readonly Type _notifyChangingInterface = typeof(INotifyPropertyChanging);

        private readonly IEntityType _entityType;
        private readonly bool _checkEquality;
        private PropertyChangingEventHandler _handler;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PropertyChangingInterceptor(
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
            var methodName = invocation.Method.Name;

            if (invocation.Method.DeclaringType.Equals(_notifyChangingInterface))
            {
                if (methodName == $"add_{nameof(INotifyPropertyChanging.PropertyChanging)}")
                {
                    _handler = (PropertyChangingEventHandler)Delegate.Combine(
                        _handler, (Delegate)invocation.Arguments[0]);
                }
                else if (methodName == $"remove_{nameof(INotifyPropertyChanging.PropertyChanging)}")
                {
                    _handler = (PropertyChangingEventHandler)Delegate.Remove(
                        _handler, (Delegate)invocation.Arguments[0]);
                }
            }
            else if (methodName.StartsWith("set_", StringComparison.Ordinal))
            {
                var propertyName = methodName.Substring(4);

                var property = _entityType.FindProperty(propertyName);
                if (property != null)
                {
                    var comparer = property.IsKeyOrForeignKey()
                        ? property.GetKeyValueComparer()
                        : property.GetValueComparer();

                    HandleChanging(invocation, property, comparer);
                }
                else
                {
                    var navigation = _entityType.FindNavigation(propertyName);
                    if (navigation != null)
                    {
                        HandleChanging(invocation, navigation, ReferenceEqualityComparer.Instance);
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

        private void HandleChanging(IInvocation invocation, IPropertyBase property, IEqualityComparer comparer)
        {
            if (_checkEquality)
            {
                var oldValue = property.GetGetter().GetClrValue(invocation.Proxy);
                var newValue = invocation.Arguments[^1];

                if (!(comparer?.Equals(oldValue, newValue) ?? Equals(oldValue, newValue)))
                {
                    NotifyPropertyChanging(property.Name, invocation.Proxy);
                }
            }
            else
            {
                NotifyPropertyChanging(property.Name, invocation.Proxy);
            }

            invocation.Proceed();
        }

        private void NotifyPropertyChanging(string propertyName, object proxy)
            => _handler?.Invoke(proxy, new PropertyChangingEventArgs(propertyName));
    }
}
