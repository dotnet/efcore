// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpRuntimeModelCodeGeneratorTest
{
    [Theory]
    [InlineData(typeof(ConstructorOnlyReaderWriter), false)]
    [InlineData(typeof(ConstructorOnlyReaderWriter), true)]
    [InlineData(typeof(SingletonReaderWriter), false)]
    [InlineData(typeof(SingletonReaderWriter), true)]
    public void Unused_default_json_readers_preserve_activation(Type readerWriterType, bool nativeAot)
    {
        var model = new RuntimeModel(
            skipDetectChanges: false,
            modelId: Guid.Empty,
            entityTypeCount: 0,
            typeConfigurationCount: 1);
        model.AddTypeMappingConfiguration(typeof(string), jsonValueReaderWriterType: readerWriterType);

        using var runtimeServices = new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider();
        runtimeServices.GetRequiredService<IModelRuntimeInitializer>().Initialize(model);

        var compiledType = CompileModel(model, nativeAot);
        var initialize = compiledType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var dependency = Assert.Single(initialize.GetCustomAttributes<DynamicDependencyAttribute>());

        Assert.Equal(readerWriterType, dependency.Type);
        Assert.Equal(
            DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties,
            dependency.MemberTypes);

        var compiledModel = (IModel)compiledType.GetProperty("Instance")!.GetValue(null)!;
        Assert.Empty(compiledModel.GetEntityTypes());
        Assert.Equal(
            readerWriterType,
            compiledModel.FindTypeMappingConfiguration(typeof(string))!.GetJsonValueReaderWriterType());

        var mapping = runtimeServices.GetRequiredService<ITypeMappingSource>().FindMapping(typeof(string), compiledModel);
        Assert.NotNull(mapping);

        var reader = mapping.JsonValueReaderWriter;
        Assert.NotNull(reader);
        Assert.Equal(readerWriterType, reader.GetType());
        Assert.Equal("\"value\"", reader.ToJsonString("value"));
        Assert.Equal("value", reader.FromJsonString("\"value\""));

        if (readerWriterType == typeof(SingletonReaderWriter))
        {
            Assert.Same(SingletonReaderWriter.Instance, reader);
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Compiled_model_preserves_element_reader_without_type_annotation(bool typeMappingDefault)
    {
        var builder = SqliteTestHelpers.Instance.CreateConventionBuilder(
            configureConventions: c =>
            {
                if (typeMappingDefault)
                {
                    c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<Int32AsStringReaderWriter>();
                }
            });
        var elementBuilder = builder.Entity<CollectionOwner>().PrimitiveCollection(e => e.Values).ElementType();
        if (!typeMappingDefault)
        {
            elementBuilder.HasJsonValueReaderWriter<Int32AsStringReaderWriter>();
        }

        var compiledType = CompileModel(builder.FinalizeModel(designTime: true), nativeAot: false);
        var compiledModel = (IModel)compiledType.GetProperty("Instance")!.GetValue(null)!;

        using var runtimeServices = new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider();
        runtimeServices.GetRequiredService<IModelRuntimeInitializer>().Initialize(compiledModel);
        var compiledProperty = compiledModel.FindEntityType(typeof(CollectionOwner))!.FindProperty(nameof(CollectionOwner.Values))!;
        var element = compiledProperty.GetElementType()!;

        Assert.Null(element[CoreAnnotationNames.JsonValueReaderWriterType]);
        var mapping = compiledProperty.GetTypeMapping();
        Assert.IsType<Int32AsStringReaderWriter>(element.GetJsonValueReaderWriter());
        var readerWriter = mapping.JsonValueReaderWriter!;
        Assert.Equal("[\"42\",\"43\"]", readerWriter.ToJsonString(new List<int> { 42, 43 }));
        Assert.Equal(new List<int> { 42, 43 }, Assert.IsAssignableFrom<IEnumerable<int>>(readerWriter.FromJsonString("[\"42\",\"43\"]")));
        if (typeMappingDefault)
        {
            Assert.Same(
                compiledModel.FindEntityType(typeof(CollectionOwner))!.FindProperty(nameof(CollectionOwner.Id))!.GetTypeMapping()
                    .JsonValueReaderWriter,
                mapping.ElementTypeMapping!.JsonValueReaderWriter);
        }
    }

    private static Type CompileModel(IModel model, bool nativeAot)
    {
        var options = new CompiledModelCodeGenerationOptions
        {
            ContextType = typeof(DbContext),
            ModelNamespace = "TestNamespace",
            ProviderName = "Microsoft.EntityFrameworkCore.Sqlite",
            ForNativeAot = nativeAot
        };

        using var designServices = (ServiceProvider)SqliteTestHelpers.Instance
            .CreateDesignServiceProvider("Microsoft.EntityFrameworkCore.Sqlite");
        var files = designServices.GetRequiredService<ICompiledModelCodeGeneratorSelector>()
            .Select(options)
            .GenerateModel(model, options);

        var build = new BuildSource { Sources = files.ToDictionary(f => f.Path, f => f.Code) };
        build.References.Add(BuildReference.ByName("System.Text.Json"));
        build.References.Add(BuildReference.ByName("System.Threading.Thread"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"));
        build.References.Add(BuildReference.ByName(typeof(CSharpRuntimeModelCodeGeneratorTest).Assembly.GetName().Name!));

        return build.BuildInMemory().GetType("TestNamespace.DbContextModel", throwOnError: true)!;
    }

    public sealed class CollectionOwner
    {
        public int Id { get; set; }
        public List<int> Values { get; set; } = [];
    }

    public sealed class Int32AsStringReaderWriter : JsonValueReaderWriter<int>
    {
        public static Int32AsStringReaderWriter Instance { get; } = new();

        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => int.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));

        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(Int32AsStringReaderWriter), nameof(Instance));
    }

    public sealed class ConstructorOnlyReaderWriter : JsonValueReaderWriter<string>
    {
        public override string FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetString()!;

        public override void ToJsonTyped(Utf8JsonWriter writer, string value)
            => writer.WriteStringValue(value);

        public override Expression ConstructorExpression
            => Expression.New(typeof(ConstructorOnlyReaderWriter));
    }

    public sealed class SingletonReaderWriter : JsonValueReaderWriter<string>
    {
        private SingletonReaderWriter()
        {
        }

        public static SingletonReaderWriter Instance { get; } = new();

        public override string FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetString()!;

        public override void ToJsonTyped(Utf8JsonWriter writer, string value)
            => writer.WriteStringValue(value);

        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(SingletonReaderWriter), nameof(Instance));
    }
}
