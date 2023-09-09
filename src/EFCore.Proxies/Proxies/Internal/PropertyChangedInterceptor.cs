// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using Castle.DynamicProxy;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertyChangedInterceptor : PropertyChangeInterceptorBase, IInterceptor
{
    private static readonly Type NotifyChangedInterface = typeof(INotifyPropertyChanged);

    private readonly bool _checkEquality;
    private PropertyChangedEventHandler? _handler;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PropertyChangedInterceptor(
        IEntityType entityType,
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

        if (invocation.Method.DeclaringType == NotifyChangedInterface)
        {
            if (methodName == $"add_{nameof(INotifyPropertyChanged.PropertyChanged)}")
            {
                _handler = (PropertyChangedEventHandler)Delegate.Combine(
                    _handler, (Delegate)invocation.Arguments[0]);
            }
            else if (methodName == $"remove_{nameof(INotifyPropertyChanged.PropertyChanged)}")
            {
                _handler = (PropertyChangedEventHandler?)Delegate.Remove(
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
                    ?? (INavigationBase?)EntityType.FindSkipNavigation(propertyName);

                if (navigation != null)
                {
                    HandleChanged(invocation, navigation, ReferenceEqualityComparer.Instance);
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

    private void HandleChanged(IInvocation invocation, IPropertyBase property, IEqualityComparer? comparer)
    {
        var newValue = invocation.Arguments[^1];

        if (_checkEquality)
        {
            var oldValue = property.GetGetter().GetClrValueUsingContainingEntity(invocation.Proxy);

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
