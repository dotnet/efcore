// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Abstract base class for <see cref="RelationalEntityMaterializer{TEntity}" />.
///     Holds include information and the entity type, providing a non-generic handle for
///     building materializer trees.
/// </remarks>
public abstract class RelationalEntityMaterializer
{
    private List<ReferenceIncludeInfo>? _referenceIncludes;
    private List<CollectionIncludeInfo>? _collectionIncludes;
    private List<JsonIncludeInfo>? _jsonIncludes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract ITypeBase StructuralType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract object? Materialize(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator);

    /// <summary>
    ///     Returns the typed <c>MaterializeTyped</c> delegate as a
    ///     <c>Func&lt;QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?&gt;</c>,
    ///     avoiding boxing when the caller knows <typeparamref name="T" /> matches the entity's CLR type.
    /// </summary>
    public Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> GetTypedMaterializeDelegate<T>()
        => Unsafe.As<Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?>>(GetMaterializeDelegateCore())!;

    /// <summary>
    ///     Returns the typed <c>MaterializeTyped</c> delegate as an untyped <c>Delegate</c>.
    ///     Overridden by the generic subclass to supply the concrete delegate.
    /// </summary>
    protected abstract Delegate GetMaterializeDelegateCore();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddReferenceInclude(ReferenceIncludeInfo includeInfo)
        => (_referenceIncludes ??= []).Add(includeInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddCollectionInclude(CollectionIncludeInfo includeInfo)
        => (_collectionIncludes ??= []).Add(includeInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ClearCollectionIncludes()
        => _collectionIncludes = null;

    /// <summary>
    ///     Whether this materializer has any collection includes (affects the multi-call protocol).
    /// </summary>
    public bool HasCollectionIncludes
        => _collectionIncludes is { Count: > 0 };

    /// <summary>
    ///     The reference includes for this entity, or null if there are none.
    /// </summary>
    public List<ReferenceIncludeInfo>? ReferenceIncludes
        => _referenceIncludes;

    /// <summary>
    ///     The collection includes for this entity, or null if there are none.
    /// </summary>
    public List<CollectionIncludeInfo>? CollectionIncludes
        => _collectionIncludes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddJsonInclude(JsonIncludeInfo includeInfo)
        => (_jsonIncludes ??= []).Add(includeInfo);

    /// <summary>
    ///     The JSON includes for this entity, or null if there are none.
    /// </summary>
    public List<JsonIncludeInfo>? JsonIncludes
        => _jsonIncludes;
}
