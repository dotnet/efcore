// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Tests for https://github.com/dotnet/efcore/issues/32352. Proxies can only override members that are visible to the
///     Castle proxy assembly (DynamicProxyGenAssembly2). A navigation whose accessor is not visible is silently never
///     intercepted, so model validation should reject it instead.
///     The entity types are emitted at runtime so that each accessibility level can be tested with and without
///     <c>InternalsVisibleTo("DynamicProxyGenAssembly2")</c> on the declaring assembly.
/// </summary>
public class ProxyAccessibilityTests
{
    // Accessors that the proxy can always override.
    public static IEnumerable<object[]> AlwaysVisible()
    {
        foreach (var grant in new[] { false, true })
        {
            yield return [MethodAttributes.Public, grant];
            yield return [MethodAttributes.Family, grant];
            yield return [MethodAttributes.FamORAssem, grant];
        }
    }

    // Accessors that the proxy can only override when the assembly grants access to DynamicProxyGenAssembly2.
    public static IEnumerable<object[]> OnlyVisibleWithGrant()
    {
        yield return [MethodAttributes.Assembly];
        yield return [MethodAttributes.FamANDAssem];
    }

    // Accessors that can never be overridden, with or without a grant.
    public static IEnumerable<object[]> NeverVisible()
    {
        foreach (var grant in new[] { false, true })
        {
            yield return [MethodAttributes.Private, grant];
        }
    }

    [Theory]
    [MemberData(nameof(AlwaysVisible))]
    public void Lazy_loading_accepts_navigation_whose_getter_is_always_visible(MethodAttributes getter, bool grant)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grant);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertNavigationMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Lazy_loading_accepts_navigation_whose_getter_is_visible_through_InternalsVisibleTo(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: true);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertNavigationMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Lazy_loading_throws_if_navigation_getter_is_not_visible_to_proxies(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: false);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertThrowsNotVisible(context, types, "getter");
    }

    [Theory]
    [MemberData(nameof(NeverVisible))]
    public void Lazy_loading_throws_if_navigation_getter_can_never_be_visible_to_proxies(MethodAttributes getter, bool grant)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grant);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertThrowsNotVisible(context, types, "getter");
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Lazy_loading_throws_if_collection_navigation_getter_is_not_visible_to_proxies(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: false, collection: true);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertThrowsNotVisible(context, types, "getter");
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Lazy_loading_accepts_collection_navigation_whose_getter_is_visible_through_InternalsVisibleTo(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: true, collection: true);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertNavigationMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Lazy_loading_does_not_throw_for_invisible_navigation_getter_if_non_virtual_navigations_are_ignored(
        MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: false);

        using var context = new AccessibilityContext(types, useLazyLoading: true, ignoreNonVirtualNavigations: true);

        AssertNavigationMapped(context, types);
    }

    [Fact]
    public void Lazy_loading_does_not_throw_for_invisible_navigation_if_lazy_loading_is_disabled_for_it()
    {
        var types = EntityFactory.Create(MethodAttributes.Assembly, MethodAttributes.Public, grantInternalsToProxies: false);

        using var context = new AccessibilityContext(
            types, useLazyLoading: true,
            configure: b => b.Entity(types.Principal).Navigation(EntityFactory.NavigationName).EnableLazyLoading(false));

        AssertNavigationMapped(context, types);
    }

    // Only the getter needs to be overridden for lazy-loading, so a less visible setter must keep working. This is the common
    // "{ get; internal set; }" pattern, including for collections.
    [Theory]
    [InlineData(MethodAttributes.Assembly, false)]
    [InlineData(MethodAttributes.Private, false)]
    [InlineData(MethodAttributes.FamANDAssem, false)]
    [InlineData(MethodAttributes.Assembly, true)]
    public void Lazy_loading_accepts_navigation_with_public_getter_and_invisible_setter(MethodAttributes setter, bool collection)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: false, collection);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertNavigationMapped(context, types);
    }

    // Castle also requires the declaring type to be accessible, but an inaccessible entity type already fails when a proxy is
    // created, so model validation must not start rejecting it with a misleading message about the (public) property.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Does_not_throw_for_public_accessors_on_inaccessible_entity_types(bool lazyLoading)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: false, publicTypes: false);

        using var context = new AccessibilityContext(types, useLazyLoading: lazyLoading, useChangeDetection: !lazyLoading);

        AssertNavigationMapped(context, types);
    }

    // The case that must keep working: an internal virtual navigation in an assembly that grants access to the proxy assembly.
    // Proves the navigation really is intercepted and lazily loaded, not just that the model builds.
    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.FamORAssem)]
    [InlineData(MethodAttributes.Public)]
    public void Lazy_loading_actually_loads_navigation_when_accessor_is_visible_to_proxies(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: true);
        var databaseName = Guid.NewGuid().ToString();

        using (var context = new AccessibilityContext(types, useLazyLoading: true, databaseName: databaseName))
        {
            var principal = Activator.CreateInstance(types.Principal)!;
            var dependent = Activator.CreateInstance(types.Dependent)!;
            EntityFactory.SetNavigation(principal, dependent);

            context.Add(principal);
            context.SaveChanges();
        }

        using (var context = new AccessibilityContext(types, useLazyLoading: true, databaseName: databaseName))
        {
            var principal = context.Query(types.Principal).Single();

            Assert.Contains("Proxy", principal.GetType().Name);
            Assert.NotNull(EntityFactory.GetNavigation(principal));
        }
    }

    // Documents the symptom from the issue: without access for the proxy assembly the navigation is never intercepted, so it
    // is never loaded. Only reachable by opting out of validation.
    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.FamANDAssem)]
    public void Navigation_is_never_loaded_when_getter_is_not_visible_to_proxies_and_validation_is_suppressed(MethodAttributes getter)
    {
        var types = EntityFactory.Create(getter, MethodAttributes.Public, grantInternalsToProxies: false);
        var databaseName = Guid.NewGuid().ToString();

        using (var context = new AccessibilityContext(
                   types, useLazyLoading: true, ignoreNonVirtualNavigations: true, databaseName: databaseName))
        {
            var principal = Activator.CreateInstance(types.Principal)!;
            EntityFactory.SetNavigation(principal, Activator.CreateInstance(types.Dependent)!);

            context.Add(principal);
            context.SaveChanges();
        }

        using (var context = new AccessibilityContext(
                   types, useLazyLoading: true, ignoreNonVirtualNavigations: true, databaseName: databaseName))
        {
            var principal = context.Query(types.Principal).Single();

            Assert.Null(EntityFactory.GetNavigation(principal));
        }
    }

    // Change-tracking proxies must override the setter, so it is the setter that must be visible.
    [Theory]
    [MemberData(nameof(AlwaysVisible))]
    public void Change_tracking_accepts_navigation_whose_setter_is_always_visible(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grant);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertNavigationMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_accepts_navigation_whose_setter_is_visible_through_InternalsVisibleTo(MethodAttributes setter)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: true);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertNavigationMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_throws_if_navigation_setter_is_not_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: false);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertThrowsNotVisible(context, types, "setter");
    }

    [Theory]
    [MemberData(nameof(NeverVisible))]
    public void Change_tracking_throws_if_navigation_setter_can_never_be_visible_to_proxies(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grant);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertThrowsNotVisible(context, types, "setter");
    }

    // "{ get; internal set; }" on a collection used to build and silently not be tracked.
    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_throws_if_collection_navigation_setter_is_not_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: false, collection: true);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertThrowsNotVisible(context, types, "setter");
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_accepts_collection_navigation_whose_setter_is_visible_through_InternalsVisibleTo(
        MethodAttributes setter)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: true, collection: true);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertNavigationMapped(context, types);
    }

    // Proves that the setter really is intercepted, and not only that the model builds.
    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.FamORAssem)]
    [InlineData(MethodAttributes.Public)]
    public void Change_tracking_detects_navigation_change_when_setter_is_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, setter, grantInternalsToProxies: true);
        var databaseName = Guid.NewGuid().ToString();

        using (var context = new AccessibilityContext(types, useChangeDetection: true, databaseName: databaseName))
        {
            var principal = context.CreateProxy(types.Principal);
            EntityFactory.SetNavigation(principal, context.CreateProxy(types.Dependent));

            context.Add(principal);
            context.SaveChanges();
        }

        using (var context = new AccessibilityContext(types, useChangeDetection: true, databaseName: databaseName))
        {
            var principal = context.Query(types.Principal).Single();
            var newDependent = context.CreateProxy(types.Dependent);
            context.Add(newDependent);

            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);

            EntityFactory.SetNavigation(principal, newDependent);

            // No call to DetectChanges: the change can only be known if the proxy intercepted the setter.
            Assert.Equal(EntityState.Modified, context.Entry(principal).State);
        }
    }

    // Scalar properties: change-tracking proxies must override the setter of every mapped property.
    [Theory]
    [MemberData(nameof(AlwaysVisible))]
    public void Change_tracking_accepts_property_whose_setter_is_always_visible(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, MethodAttributes.Public, grant, scalarSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertPropertyMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_accepts_property_whose_setter_is_visible_through_InternalsVisibleTo(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: true, scalarSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertPropertyMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_throws_if_property_setter_is_not_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: false, scalarSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        Assert.Equal(
            ProxiesStrings.NonVisibleProperty(EntityFactory.PropertyName, types.Principal.Name, "setter"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Theory]
    [MemberData(nameof(NeverVisible))]
    public void Change_tracking_throws_if_property_setter_can_never_be_visible_to_proxies(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, MethodAttributes.Public, grant, scalarSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        Assert.Equal(
            ProxiesStrings.NonVisibleProperty(EntityFactory.PropertyName, types.Principal.Name, "setter"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    // Lazy-loading proxies do not intercept scalar properties, so their visibility is irrelevant.
    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.Private)]
    [InlineData(MethodAttributes.FamANDAssem)]
    public void Lazy_loading_does_not_validate_property_setter_visibility(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: false, scalarSetter: setter);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertPropertyMapped(context, types);
    }

    // Proves that the setter really is intercepted, and not only that the model builds. This is the silent data loss from
    // https://github.com/dotnet/efcore/issues/39589 when the setter is not visible.
    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.FamORAssem)]
    [InlineData(MethodAttributes.Public)]
    public void Change_tracking_detects_property_change_when_setter_is_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: true, scalarSetter: setter);
        var databaseName = Guid.NewGuid().ToString();

        using (var context = new AccessibilityContext(types, useChangeDetection: true, databaseName: databaseName))
        {
            var principal = context.CreateProxy(types.Principal);
            EntityFactory.SetProperty(principal, EntityFactory.PropertyName, "a");

            context.Add(principal);
            context.SaveChanges();
        }

        using (var context = new AccessibilityContext(types, useChangeDetection: true, databaseName: databaseName))
        {
            var principal = context.Query(types.Principal).Single();

            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);

            EntityFactory.SetProperty(principal, EntityFactory.PropertyName, "b");

            // Entry(...).State does not compare snapshots for change-tracking proxies: they use notifications only, so the
            // entity can only be Modified here if the proxy intercepted the setter and raised the notification.
            Assert.Equal(EntityState.Modified, context.Entry(principal).State);
        }
    }

    // Indexer properties: the indexer setter must be overridable too.
    [Theory]
    [MemberData(nameof(AlwaysVisible))]
    public void Change_tracking_accepts_indexer_whose_setter_is_always_visible(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, MethodAttributes.Public, grant, indexerSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertIndexerMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_accepts_indexer_whose_setter_is_visible_through_InternalsVisibleTo(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: true, indexerSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        AssertIndexerMapped(context, types);
    }

    [Theory]
    [MemberData(nameof(OnlyVisibleWithGrant))]
    public void Change_tracking_throws_if_indexer_setter_is_not_visible_to_proxies(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: false, indexerSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        Assert.Equal(
            ProxiesStrings.NonVisibleIndexerProperty(types.Principal.Name),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Theory]
    [MemberData(nameof(NeverVisible))]
    public void Change_tracking_throws_if_indexer_setter_can_never_be_visible_to_proxies(MethodAttributes setter, bool grant)
    {
        var types = EntityFactory.Create(MethodAttributes.Public, MethodAttributes.Public, grant, indexerSetter: setter);

        using var context = new AccessibilityContext(types, useChangeDetection: true);

        Assert.Equal(
            ProxiesStrings.NonVisibleIndexerProperty(types.Principal.Name),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Theory]
    [InlineData(MethodAttributes.Assembly)]
    [InlineData(MethodAttributes.Public)]
    public void Lazy_loading_does_not_validate_indexer_setter_visibility(MethodAttributes setter)
    {
        var types = EntityFactory.Create(
            MethodAttributes.Public, MethodAttributes.Public, grantInternalsToProxies: false, indexerSetter: setter);

        using var context = new AccessibilityContext(types, useLazyLoading: true);

        AssertIndexerMapped(context, types);
    }

    private static void AssertPropertyMapped(DbContext context, EntityFactory.Types types)
        => Assert.NotNull(context.Model.FindEntityType(types.Principal)!.FindProperty(EntityFactory.PropertyName));

    private static void AssertIndexerMapped(DbContext context, EntityFactory.Types types)
        => Assert.NotNull(context.Model.FindEntityType(types.Principal)!.FindProperty(EntityFactory.IndexerPropertyName));

    private static void AssertNavigationMapped(DbContext context, EntityFactory.Types types)
        => Assert.NotNull(context.Model.FindEntityType(types.Principal)!.FindNavigation(EntityFactory.NavigationName));

    private static void AssertThrowsNotVisible(DbContext context, EntityFactory.Types types, string accessor)
        => Assert.Equal(
            ProxiesStrings.NonVisibleProperty(EntityFactory.NavigationName, types.Principal.Name, accessor),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);

    private sealed class AccessibilityContext(
        EntityFactory.Types types,
        bool useLazyLoading = false,
        bool useChangeDetection = false,
        bool ignoreNonVirtualNavigations = false,
        string? databaseName = null,
        Action<ModelBuilder>? configure = null)
        : DbContext
    {
        private static readonly InMemoryDatabaseRoot _dbRoot = new();

        private readonly string _databaseName = databaseName ?? Guid.NewGuid().ToString();

        private readonly IServiceProvider _internalServiceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddEntityFrameworkProxies()
            .BuildServiceProvider(validateScopes: true);

        public IQueryable<object> Query(Type entityType)
            => (IQueryable<object>)typeof(DbContext).GetMethods()
                .Single(m => m.Name == nameof(Set) && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(entityType)
                .Invoke(this, null)!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (useLazyLoading)
            {
                optionsBuilder.UseLazyLoadingProxies(b =>
                {
                    if (ignoreNonVirtualNavigations)
                    {
                        b.IgnoreNonVirtualNavigations();
                    }
                });
            }

            if (useChangeDetection)
            {
                optionsBuilder.UseChangeTrackingProxies();
            }

            optionsBuilder
                .UseInternalServiceProvider(_internalServiceProvider)
                .UseInMemoryDatabase(_databaseName, _dbRoot);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Non-public navigations are not found by convention, so they are always configured explicitly.
            var entityTypeBuilder = modelBuilder.Entity(types.Principal);

            // Properties whose setters are not public are not found by convention either.
            entityTypeBuilder.Property(EntityFactory.PropertyName);

            if (types.HasIndexer)
            {
                entityTypeBuilder.IndexerProperty(typeof(int), EntityFactory.IndexerPropertyName);
            }

            if (types.IsCollection)
            {
                entityTypeBuilder.HasMany(types.Dependent, EntityFactory.NavigationName).WithOne();
            }
            else
            {
                entityTypeBuilder.HasOne(types.Dependent, EntityFactory.NavigationName).WithMany();
            }

            configure?.Invoke(modelBuilder);
        }
    }

    private static class EntityFactory
    {
        public const string NavigationName = "Nav";
        public const string PropertyName = "Name";
        public const string IndexerPropertyName = "Snoopy";

        public sealed record Types(Type Principal, Type Dependent, bool IsCollection, bool HasIndexer);

        public static Types Create(
            MethodAttributes getter,
            MethodAttributes setter,
            bool grantInternalsToProxies,
            bool collection = false,
            bool publicTypes = true,
            MethodAttributes scalarSetter = MethodAttributes.Public,
            MethodAttributes? indexerSetter = null)
        {
            var assemblyName = "ProxyAccessibility_" + Guid.NewGuid().ToString("N");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);

            if (grantInternalsToProxies)
            {
                assembly.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(InternalsVisibleToAttribute).GetConstructor([typeof(string)])!,
                        ["DynamicProxyGenAssembly2"]));
            }

            var module = assembly.DefineDynamicModule(assemblyName);
            var typeVisibility = (publicTypes ? TypeAttributes.Public : TypeAttributes.NotPublic) | TypeAttributes.Class;

            var dependent = module.DefineType("Dependent", typeVisibility);
            dependent.DefineDefaultConstructor(MethodAttributes.Public);
            DefineProperty(dependent, "Id", typeof(int), MethodAttributes.Public, MethodAttributes.Public);
            var dependentType = dependent.CreateType();

            var principal = module.DefineType("Principal", typeVisibility);
            principal.DefineDefaultConstructor(MethodAttributes.Public);
            DefineProperty(principal, "Id", typeof(int), MethodAttributes.Public, MethodAttributes.Public);
            DefineProperty(principal, PropertyName, typeof(string), MethodAttributes.Public, scalarSetter);

            if (indexerSetter.HasValue)
            {
                DefineIndexer(principal, MethodAttributes.Public, indexerSetter.Value);
            }

            DefineProperty(
                principal,
                NavigationName,
                collection ? typeof(ICollection<>).MakeGenericType(dependentType) : dependentType,
                getter,
                setter);
            var principalType = principal.CreateType();

            return new Types(principalType, dependentType, collection, indexerSetter.HasValue);
        }

        public static object? GetNavigation(object principal)
            => FindProperty(principal, NavigationName).GetValue(principal);

        public static void SetNavigation(object principal, object dependent)
            => FindProperty(principal, NavigationName).SetValue(principal, dependent);

        public static void SetProperty(object principal, string name, object? value)
            => FindProperty(principal, name).SetValue(principal, value);

        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // The instance may be a proxy subclass, which cannot see the non-public accessors declared on its base type.
        private static PropertyInfo FindProperty(object principal, string name)
        {
            for (var type = principal.GetType(); type != null; type = type.BaseType)
            {
                var property = type.GetProperty(name, AnyInstance | BindingFlags.DeclaredOnly);

                // A proxy redeclares the property with only the accessors it could override, so skip incomplete declarations.
                if (property?.GetGetMethod(nonPublic: true) != null
                    && property.GetSetMethod(nonPublic: true) != null)
                {
                    return property;
                }
            }

            throw new InvalidOperationException($"Property '{name}' not found.");
        }

        // An indexer of type "object this[string key]" with trivial accessors: it is only used to build models.
        private static void DefineIndexer(TypeBuilder type, MethodAttributes getterVisibility, MethodAttributes setterVisibility)
        {
            type.SetCustomAttribute(
                new CustomAttributeBuilder(typeof(DefaultMemberAttribute).GetConstructor([typeof(string)])!, ["Item"]));

            var property = type.DefineProperty("Item", PropertyAttributes.None, typeof(object), [typeof(string)]);

            const MethodAttributes common = MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName;

            var get = type.DefineMethod("get_Item", getterVisibility | common, typeof(object), [typeof(string)]);
            var getIl = get.GetILGenerator();
            getIl.Emit(OpCodes.Ldnull);
            getIl.Emit(OpCodes.Ret);

            var set = type.DefineMethod("set_Item", setterVisibility | common, typeof(void), [typeof(string), typeof(object)]);
            set.GetILGenerator().Emit(OpCodes.Ret);

            property.SetGetMethod(get);
            property.SetSetMethod(set);
        }

        private static void DefineProperty(
            TypeBuilder type,
            string name,
            Type propertyType,
            MethodAttributes getterVisibility,
            MethodAttributes setterVisibility)
        {
            var field = type.DefineField("_" + name, propertyType, FieldAttributes.Private);
            var property = type.DefineProperty(name, PropertyAttributes.None, propertyType, null);

            const MethodAttributes common = MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName;

            var get = type.DefineMethod("get_" + name, getterVisibility | common, propertyType, Type.EmptyTypes);
            var getIl = get.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            var set = type.DefineMethod("set_" + name, setterVisibility | common, typeof(void), [propertyType]);
            var setIl = set.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            property.SetGetMethod(get);
            property.SetSetMethod(set);
        }
    }
}
