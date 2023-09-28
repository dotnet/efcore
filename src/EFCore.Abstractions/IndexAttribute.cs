// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Specifies an index to be generated in the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IndexAttribute : Attribute
{
    private string? _name;
    private bool? _isUnique;
    private bool[]? _isDescending;
    private bool _allDescending;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexAttribute" /> class.
    /// </summary>
    /// <param name="propertyName">The first (or only) property in the index.</param>
    /// <param name="additionalPropertyNames">The additional properties which constitute the index, if any, in order.</param>
    public IndexAttribute(string propertyName, params string[] additionalPropertyNames)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));
        Check.HasNoEmptyElements(additionalPropertyNames, nameof(additionalPropertyNames));

        PropertyNames = new List<string> { propertyName };
        ((List<string>)PropertyNames).AddRange(additionalPropertyNames);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexAttribute" /> class.
    /// </summary>
    /// <param name="propertyNames">The properties which constitute the index, in order (there must be at least one).</param>
    [Obsolete("Use the other constructor")]
    public IndexAttribute(params string[] propertyNames)
    {
        Check.NotEmpty(propertyNames, nameof(propertyNames));
        Check.HasNoEmptyElements(propertyNames, nameof(propertyNames));

        PropertyNames = propertyNames.ToList();
    }

    /// <summary>
    ///     The properties which constitute the index, in order.
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; }

    /// <summary>
    ///     The name of the index.
    /// </summary>
    [DisallowNull]
    public string? Name
    {
        get => _name;
        set => _name = Check.NotNull(value, nameof(value));
    }

    /// <summary>
    ///     Whether the index is unique.
    /// </summary>
    public bool IsUnique
    {
        get => _isUnique ?? false;
        set => _isUnique = value;
    }

    /// <summary>
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    /// </summary>
    public bool[]? IsDescending
    {
        get => _isDescending;
        set
        {
            if (value is not null)
            {
                if (value.Length != PropertyNames.Count)
                {
                    throw new ArgumentException(
                        AbstractionsStrings.InvalidNumberOfIndexSortOrderValues(value.Length, PropertyNames.Count), nameof(IsDescending));
                }

                if (_allDescending)
                {
                    throw new ArgumentException(AbstractionsStrings.CannotSpecifyBothIsDescendingAndAllDescending);
                }
            }

            _isDescending = value;
        }
    }

    /// <summary>
    ///     Whether all index columns have descending sort order.
    /// </summary>
    public bool AllDescending
    {
        get => _allDescending;
        set
        {
            if (IsDescending is not null)
            {
                throw new ArgumentException(AbstractionsStrings.CannotSpecifyBothIsDescendingAndAllDescending);
            }

            _allDescending = value;
        }
    }

    /// <summary>
    ///     Checks whether <see cref="IsUnique" /> has been explicitly set to a value.
    /// </summary>
    public bool IsUniqueHasValue
        => _isUnique.HasValue;
}
