// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ModelBuilding101TestBase
{
    protected virtual void Model101Test([CallerMemberName] string testMame = "")
    {
        var models = new List<ModelMetadata>();
        var testTypeName = "Microsoft.EntityFrameworkCore.ModelBuilding101TestBase+" + testMame.Substring(0, testMame.Length - 4);

        foreach (Context101 context in Type.GetType(testTypeName, throwOnError: true)!.GetNestedTypes()
                     .Where(t => t.IsAssignableTo(typeof(DbContext)))
                     .Select(Activator.CreateInstance))
        {
            context.ConfigureAction = b => ConfigureContext(b);
            models.Add(GetModelMetadata(context));
            context.Dispose();
        }

        Assert.True(models.Count >= 2);

        for (var i = 1; i < models.Count; i++)
        {
            Assert.Equal(models[0], models[i]);
        }
    }

    protected virtual ModelMetadata GetModelMetadata(Context101 context)
        => new(context.Model);

    protected class ModelMetadata(IModel model)
    {
        public virtual IModel Model { get; } = model;
        public virtual string ModelDebugView { get; } = model.ToDebugString();

        protected bool Equals(ModelMetadata other)
            => ModelDebugView == other.ModelDebugView;

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
                && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType()
                    && Equals((ModelMetadata)obj));

        public override int GetHashCode()
            => ModelDebugView.GetHashCode();
    }

    protected abstract class Context101 : DbContext
    {
        public virtual Action<DbContextOptionsBuilder> ConfigureAction { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => ConfigureAction(optionsBuilder);
    }

    protected abstract DbContextOptionsBuilder ConfigureContext(DbContextOptionsBuilder optionsBuilder);
}
