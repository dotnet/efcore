// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class NavigationTest
{
    [ConditionalFact]
    public void Can_create_navigation()
    {
        var foreignKey = CreateForeignKey();

        var navigation = foreignKey.SetDependentToPrincipal(E.DeceptionProperty);

        Assert.Same(foreignKey, navigation.ForeignKey);
        Assert.Equal(nameof(E.Deception), navigation.Name);
        Assert.Same(foreignKey.DeclaringEntityType, navigation.DeclaringEntityType);
    }

    [ConditionalFact]
    public virtual void Detects_navigations_to_keyless_types()
    {
        IMutableModel model = new Model();
        var entityType = model.AddEntityType(typeof(B));
        var idProperty = entityType.AddProperty("id", typeof(int));
        var key = entityType.SetPrimaryKey(idProperty);
        var keylessType = model.AddEntityType(typeof(A));
        keylessType.IsKeyless = true;
        var fkProperty = keylessType.AddProperty("p", typeof(int));
        var fk = keylessType.AddForeignKey(fkProperty, key, entityType);
        Assert.Equal(
            CoreStrings.NavigationToKeylessType(nameof(B.ManyAs), nameof(A)),
            Assert.Throws<InvalidOperationException>(() => fk.SetPrincipalToDependent(nameof(B.ManyAs))).Message);
    }

    private IMutableForeignKey CreateForeignKey()
    {
        IMutableModel model = new Model();
        var entityType = model.AddEntityType(typeof(E));
        var idProperty = entityType.AddProperty("id", typeof(int));
        var key = entityType.SetPrimaryKey(idProperty);
        var fkProperty = entityType.AddProperty("p", typeof(int));
        return entityType.AddForeignKey(fkProperty, key, entityType);
    }

    protected class A
    {
        public int Id { get; set; }

        public int? P0 { get; set; }
        public int? P1 { get; set; }
        public int? P2 { get; set; }
        public int? P3 { get; set; }
    }

    protected class B
    {
        public int Id { get; set; }

        public int? P0 { get; set; }
        public int? P1 { get; set; }
        public int? P2 { get; set; }
        public int? P3 { get; set; }

        public A A { get; set; }
        public A AnotherA { get; set; }
        public ICollection<A> ManyAs { get; set; }
    }

    private class E
    {
        public static readonly PropertyInfo DeceptionProperty = typeof(E).GetProperty(nameof(Deception));

        public E Deception { get; set; }
    }
}
