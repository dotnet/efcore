// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Static methods that are useful in application code where there is not an EF type for the method to be accessed from. For example,
///     referencing a shadow state property in a LINQ query.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> and
///     <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information and examples.
/// </remarks>
// ReSharper disable once InconsistentNaming
public static partial class EF
{
    internal static readonly MethodInfo PropertyMethod
        = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property))!;

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "EF.Property has no DynamicallyAccessedMembers annotations and is safe to construct.")]
    internal static MethodInfo MakePropertyMethod(Type type)
        => PropertyMethod.MakeGenericMethod(type);

    /// <summary>
    ///     This flag is set to <see langword="true" /> when code is being run from a design-time tool, such
    ///     as "dotnet ef" or one of the Package Manager Console PowerShell commands "Add-Migration", "Update-Database", etc.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This flag can be inspected to change application behavior. For example, if the application is being executed by an EF
    ///         design-time tool, then it may choose to skip executing migrations commands as part of startup.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-commandline">EF Core command-line reference </see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    public static bool IsDesignTime { get; set; }

    /// <summary>
    ///     References a given property or navigation on an entity or complex type instance. This is useful for shadow state properties, for
    ///     which no CLR property exists. Currently this method can only be used in LINQ queries and can not be used to
    ///     access the value assigned to a property in other scenarios.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this is a static method accessed through the top-level <see cref="EF" /> static type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being referenced.</typeparam>
    /// <param name="instance">The entity or complex type to access the property on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value assigned to the property.</returns>
    public static TProperty Property<TProperty>(
        object instance,
        [NotParameterized] string propertyName)
        => throw new InvalidOperationException(CoreStrings.PropertyMethodInvoked);

    /// <summary>
    ///     Within the context of an EF LINQ query, forces its argument to be inserted into the query as a constant expression. This can be
    ///     used to e.g. integrate a value as a constant inside an EF query, instead of as a parameter, for query performance reasons.
    /// </summary>
    /// <remarks>Note that this is a static method accessed through the top-level <see cref="EF" /> static type.</remarks>
    /// <typeparam name="T">The type of the expression to be integrated as a constant into the query.</typeparam>
    /// <param name="argument">The expression to be integrated as a constant into the query.</param>
    /// <returns>The same value for further use in the query.</returns>
    public static T Constant<T>(T argument)
        => throw new InvalidOperationException(CoreStrings.EFConstantInvoked);

    /// <summary>
    ///     Within the context of an EF LINQ query, forces its argument to be inserted into the query as a parameter expression. This can be
    ///     used to e.g. make sure a constant value is parameterized instead of integrated as a constant into the query, which can be useful
    ///     in dynamic query construction scenarios.
    /// </summary>
    /// <remarks>Note that this is a static method accessed through the top-level <see cref="EF" /> static type.</remarks>
    /// <typeparam name="T">The type of the expression to be integrated as a parameter into the query.</typeparam>
    /// <param name="argument">The expression to be integrated as a parameter into the query.</param>
    /// <returns>The same value for further use in the query.</returns>
    public static T Parameter<T>(T argument)
        => throw new InvalidOperationException(CoreStrings.EFParameterInvoked);

    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     Calling these methods in other contexts (e.g. LINQ to Objects) will throw a <see cref="NotSupportedException" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this is a static property accessed through the top-level <see cref="EF" /> static type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public static DbFunctions Functions
        => DbFunctions.Instance;
}
