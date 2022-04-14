// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for
///     incompatible foreign key properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ForeignKeyCandidateEventData : TwoPropertyBaseCollectionsEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="dependentToPrincipalNavigationSpecification">
    ///     The name of the navigation property or entity type on the dependent end of the
    ///     relationship.
    /// </param>
    /// <param name="principalToDependentNavigationSpecification">
    ///     The name of the navigation property or entity type on the principal end of the
    ///     relationship.
    /// </param>
    /// <param name="firstPropertyCollection">The first property collection.</param>
    /// <param name="secondPropertyCollection">The second property collection.</param>
    public ForeignKeyCandidateEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        string dependentToPrincipalNavigationSpecification,
        string principalToDependentNavigationSpecification,
        IReadOnlyList<IReadOnlyPropertyBase> firstPropertyCollection,
        IReadOnlyList<IReadOnlyPropertyBase> secondPropertyCollection)
        : base(eventDefinition, messageGenerator, firstPropertyCollection, secondPropertyCollection)
    {
        DependentToPrincipalNavigationSpecification = dependentToPrincipalNavigationSpecification;
        PrincipalToDependentNavigationSpecification = principalToDependentNavigationSpecification;
    }

    /// <summary>
    ///     The name of the navigation property or entity type on the dependent end of the relationship.
    /// </summary>
    public virtual string DependentToPrincipalNavigationSpecification { get; }

    /// <summary>
    ///     The name of the navigation property or entity type on the principal end of the relationship.
    /// </summary>
    public virtual string PrincipalToDependentNavigationSpecification { get; }
}
