// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Classifies CLR members of a type during model building, determining whether a member is a candidate
///     navigation, scalar property, complex property, or service property.
/// </summary>
/// <remarks>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
/// </remarks>
public class MemberClassifier : IMemberClassifier
{
    /// <summary>
    ///     Creates a new instance of <see cref="MemberClassifier" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public MemberClassifier(MemberClassifierDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual MemberClassifierDependencies Dependencies { get; }

    /// <summary>
    ///     Returns the kind of structural type the given CLR type has been explicitly configured as, or
    ///     <see langword="null" /> if it has no explicit configuration.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <param name="model">The model.</param>
    /// <returns>The configuration type, or <see langword="null" />.</returns>
    protected virtual TypeConfigurationType? GetConfigurationType(Type type, IConventionModel model)
        => ((Model)model).Configuration?.GetConfigurationType(type);

    /// <inheritdoc />
    public virtual bool IsCandidateNavigationProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out Type? elementType,
        out bool? shouldBeOwned,
        out bool explicitlyConfigured)
    {
        shouldBeOwned = null;
        explicitlyConfigured = false;
        elementType = null;

        var propertyInfo = memberInfo as PropertyInfo;
        var targetType = memberInfo.GetMemberType();
        var targetSequenceType = targetType.TryGetSequenceType();

        if (targetSequenceType != null
            && (propertyInfo == null
                || propertyInfo.IsCandidateProperty(needsWrite: false))
            && IsCandidateNavigationPropertyType(targetSequenceType, memberInfo, model, useAttributes, out shouldBeOwned, out explicitlyConfigured))
        {
            elementType = targetSequenceType;
            return true;
        }

        if ((propertyInfo == null
                || propertyInfo.IsCandidateProperty(needsWrite: true))
            && IsCandidateNavigationPropertyType(targetType, memberInfo, model, useAttributes, out shouldBeOwned, out explicitlyConfigured))
        {
            // This is a non-collection (reference) navigation, so there is no element type; callers that need the
            // navigation target type use memberInfo.GetMemberType(). elementType remains null.
            return true;
        }

        return false;
    }

    private bool IsCandidateNavigationPropertyType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type targetType,
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out bool? shouldBeOwned,
        out bool explicitlyConfigured)
    {
        shouldBeOwned = null;
        var configurationType = GetConfigurationType(targetType, model);
        explicitlyConfigured = configurationType != null;
        var isConfiguredAsEntityType = configurationType.IsEntityType();
        if (isConfiguredAsEntityType == false
            || !targetType.IsValidEntityType())
        {
            return false;
        }

        if (configurationType != null)
        {
            shouldBeOwned = configurationType == TypeConfigurationType.OwnedEntityType;
        }

        var memberType = memberInfo.GetMemberType();
        return isConfiguredAsEntityType == true
            || targetType != typeof(object)
            && (memberType != targetType
                || (Dependencies.ParameterBindingFactories.FindFactory(memberType, memberInfo.GetSimpleMemberName()) == null
                    && Dependencies.TypeMappingSource.FindMapping(memberInfo, (IModel)model, useAttributes) == null));
    }

    /// <inheritdoc />
    public virtual bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out CoreTypeMapping? typeMapping,
        out bool explicitlyConfigured)
    {
        typeMapping = null;
        explicitlyConfigured = false;
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var configurationType = GetConfigurationType(memberInfo.GetMemberType(), model);
        explicitlyConfigured = configurationType != null;
        return configurationType == TypeConfigurationType.Property
            || (configurationType == null
                && (typeMapping = Dependencies.TypeMappingSource.FindMapping(memberInfo, (IModel)model, useAttributes)) != null);
    }

    /// <inheritdoc />
    public virtual bool IsCandidateComplexProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out Type? elementType,
        out bool explicitlyConfigured)
    {
        explicitlyConfigured = false;
        elementType = null;
        if (!memberInfo.IsCandidateProperty())
        {
            return false;
        }

        var targetType = memberInfo.GetMemberType();
        if (targetType.TryGetElementType(typeof(IList<>)) is { } sequenceType
            && IsCandidateComplexType(sequenceType.UnwrapNullableType(), model, out explicitlyConfigured))
        {
            elementType = sequenceType;
            return true;
        }

        return IsCandidateComplexType(targetType.UnwrapNullableType(), model, out explicitlyConfigured);
    }

    private bool IsCandidateComplexType(Type targetType, IConventionModel model, out bool explicitlyConfigured)
    {
        if (!targetType.IsValidComplexType()
            || (targetType.IsGenericType
                && targetType.GetGenericTypeDefinition() is var genericTypeDefinition
                && (genericTypeDefinition == typeof(Dictionary<,>)
                    || genericTypeDefinition == typeof(List<>)
                    || genericTypeDefinition == typeof(HashSet<>)
                    || genericTypeDefinition == typeof(Collection<>)
                    || genericTypeDefinition == typeof(ObservableCollection<>))))
        {
            explicitlyConfigured = false;
            return false;
        }

        var configurationType = GetConfigurationType(targetType, model);
        explicitlyConfigured = configurationType != null;
        return configurationType == TypeConfigurationType.ComplexType
            || configurationType == null;
    }

    /// <inheritdoc />
    public virtual bool IsCandidateServiceProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out IParameterBindingFactory? bindingFactory,
        out bool explicitlyConfigured)
    {
        bindingFactory = null;
        explicitlyConfigured = false;
        if (!memberInfo.IsCandidateProperty(publicOnly: false))
        {
            return false;
        }

        var type = memberInfo.GetMemberType();
        var configurationType = GetConfigurationType(type, model);
        explicitlyConfigured = configurationType != null;
        if (configurationType != TypeConfigurationType.ServiceProperty)
        {
            if (configurationType != null)
            {
                return false;
            }

            if (memberInfo.IsCandidateProperty()
                && Dependencies.TypeMappingSource.FindMapping(memberInfo, (IModel)model, useAttributes) != null)
            {
                return false;
            }
        }

        bindingFactory = Dependencies.ParameterBindingFactories.FindFactory(type, memberInfo.GetSimpleMemberName());
        return bindingFactory != null;
    }
}
