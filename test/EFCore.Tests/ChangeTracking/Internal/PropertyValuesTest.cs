// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class PropertyValuesTest
{
    [ConditionalFact]
    public void Can_safely_get_originalvalue_and_currentvalue_with_TryGetValue()
    {
        var stateManager = CreateStateManager(mb => mb.Entity<SimpleEntity>());

        const string NameValue = "Simple Name";
        const string NewNameValue = "A New Name";

        var entity = new SimpleEntity { Name = NameValue };

        var entityEntry = stateManager.GetOrCreateEntry(entity);
        entityEntry.SetEntityState(EntityState.Unchanged);
        var entry = new EntityEntry<SimpleEntity>(entityEntry);

        entity.Name = NewNameValue;

        var current = entry.CurrentValues.TryGetValue<string>("Name", out var currentName);
        var original = entry.OriginalValues.TryGetValue<string>("Name", out var originalName);

        Assert.True(current);
        Assert.True(original);

        Assert.Equal(NameValue, originalName);
        Assert.Equal(NewNameValue, currentName);
    }

    [ConditionalFact]
    public void TryGetValue_should_not_throw_error_when_property_does_not_exist()
    {
        var stateManager = CreateStateManager(mb => mb.Entity<SimpleEntity>());

        const string NameValue = "Simple Name";
        const string NewNameValue = "A New Name";

        var entity = new SimpleEntity { Name = NameValue };

        var entityEntry = stateManager.GetOrCreateEntry(entity);
        entityEntry.SetEntityState(EntityState.Unchanged);
        var entry = new EntityEntry<SimpleEntity>(entityEntry);

        entity.Name = NewNameValue;

        var current = entry.CurrentValues.TryGetValue<string>("Non_Existent_Property", out var non_existent_current);
        var original = entry.OriginalValues.TryGetValue<string>("Non_Existent_Property", out var non_existent_original);

        Assert.False(current);
        Assert.False(original);

        Assert.Null(non_existent_current);
        Assert.Null(non_existent_original);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool?>))]
    public void ToObject_with_null_complex_property(bool? useOriginalValues)
    {
        var job = new Job { Id = 1, Name = "Job with No Error" };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Job with No Error", result.Name);
        Assert.Null(result.Error);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool?>))]
    public void ToObject_with_all_nested_complex_properties_populated(bool? useOriginalValues)
    {
        var job = new Job
        {
            Id = 1,
            Name = "Job with Error + Inner Errors",
            Error = new RootJobError
            {
                Code = "500",
                Message = "Internal Server Error",
                InnerError = new JobError
                {
                    Code = "501",
                    Message = "Not Implemented",
                    InnerError = new LeafJobError
                    {
                        Code = "502",
                        Message = "Bad Gateway"
                    }
                }
            }
        };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Job with Error + Inner Errors", result.Name);
        Assert.NotNull(result.Error);
        Assert.Equal("500", result.Error!.Code);
        Assert.NotNull(result.Error.InnerError);
        Assert.Equal("501", result.Error.InnerError!.Code);
        Assert.NotNull(result.Error.InnerError.InnerError);
        Assert.Equal("502", result.Error.InnerError.InnerError!.Code);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool?>))]
    public void ToObject_with_null_nested_complex_property(bool? useOriginalValues)
    {
        var job = new Job
        {
            Id = 1,
            Name = "Job with Error only",
            Error = new RootJobError
            {
                Code = "400",
                Message = "Bad Request"
            }
        };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Job with Error only", result.Name);
        Assert.NotNull(result.Error);
        Assert.Equal("400", result.Error!.Code);
        Assert.Null(result.Error.InnerError);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool?>))]
    public void ToObject_with_complex_collection_containing_nested_nullable_complex_properties(
        bool? useOriginalValues)
    {
        var job = new Job
        {
            Id = 1,
            Name = "Test Job",
            Errors =
            [
                new RootJobError { Code = "400", Message = "Bad Request", InnerError = null },
                new RootJobError
                {
                    Code = "500",
                    Message = "Server Error",
                    InnerError = new JobError { Code = "501", Message = "Not Implemented", InnerError = null }
                },
                new RootJobError
                {
                    Code = "503",
                    Message = "Service Unavailable",
                    InnerError = new JobError
                    {
                        Code = "504",
                        Message = "Gateway Timeout",
                        InnerError = new LeafJobError { Code = "505", Message = "Internal Error" }
                    }
                }
            ]
        };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Test Job", result.Name);
        Assert.Null(result.Error);
        Assert.NotNull(result.Errors);
        Assert.Equal(3, result.Errors.Count);

        Assert.Equal("400", result.Errors[0].Code);
        Assert.Equal("Bad Request", result.Errors[0].Message);
        Assert.Null(result.Errors[0].InnerError);

        Assert.Equal("500", result.Errors[1].Code);
        Assert.Equal("Server Error", result.Errors[1].Message);
        Assert.NotNull(result.Errors[1].InnerError);
        Assert.Equal("501", result.Errors[1].InnerError!.Code);
        Assert.Null(result.Errors[1].InnerError!.InnerError);

        Assert.Equal("503", result.Errors[2].Code);
        Assert.NotNull(result.Errors[2].InnerError);
        Assert.Equal("504", result.Errors[2].InnerError!.Code);
        Assert.NotNull(result.Errors[2].InnerError!.InnerError);
        Assert.Equal("505", result.Errors[2].InnerError!.InnerError!.Code);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool>))]
    public void ToObject_with_null_complex_property_in_complex_collection_in_complex_collection(
        bool? useOriginalValues)
    {
        var job = new Job
        {
            Id = 1,
            Name = "Double Collection Test",
            Errors =
            [
                new RootJobError
                {
                    Code = "400",
                    Message = "Bad Request",
                    InnerErrors =
                    [
                        new JobError { Code = "401", Message = "Unauthorized", InnerError = null },
                        new JobError
                        {
                            Code = "402",
                            Message = "Payment Required",
                            InnerError = new LeafJobError { Code = "403", Message = "Forbidden" }
                        }
                    ]
                }
            ]
        };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Double Collection Test", result.Name);
        Assert.Null(result.Error);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);

        var error = result.Errors[0];
        Assert.Equal("400", error.Code);
        Assert.Null(error.InnerError);
        Assert.NotNull(error.InnerErrors);
        Assert.Equal(2, error.InnerErrors.Count);

        Assert.Equal("401", error.InnerErrors[0].Code);
        Assert.Null(error.InnerErrors[0].InnerError);

        Assert.Equal("402", error.InnerErrors[1].Code);
        Assert.NotNull(error.InnerErrors[1].InnerError);
        Assert.Equal("403", error.InnerErrors[1].InnerError!.Code);
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool?>))]
    public void ToObject_with_null_complex_property_in_complex_collection_element_in_complex_property(
        bool? useOriginalValues)
    {
        var job = new Job
        {
            Id = 1,
            Name = "Nested Collection Test",
            Error = new RootJobError
            {
                Code = "500",
                Message = "Server Error",
                InnerErrors =
                [
                    new JobError { Code = "501", Message = "Not Implemented", InnerError = null },
                    new JobError
                    {
                        Code = "502",
                        Message = "Bad Gateway",
                        InnerError = new LeafJobError { Code = "503", Message = "Service Unavailable" }
                    }
                ]
            }
        };

        var result = GetToObjectResult(job, useOriginalValues);

        Assert.Equal("Nested Collection Test", result.Name);
        Assert.NotNull(result.Error);
        Assert.Equal("500", result.Error!.Code);
        Assert.Null(result.Error.InnerError);
        Assert.NotNull(result.Error.InnerErrors);
        Assert.Equal(2, result.Error.InnerErrors.Count);

        Assert.Equal("501", result.Error.InnerErrors[0].Code);
        Assert.Null(result.Error.InnerErrors[0].InnerError);

        Assert.Equal("502", result.Error.InnerErrors[1].Code);
        Assert.NotNull(result.Error.InnerErrors[1].InnerError);
        Assert.Equal("503", result.Error.InnerErrors[1].InnerError!.Code);
    }

    #region Helpers

    private static IStateManager CreateStateManager(Action<ModelBuilder> buildModel)
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        buildModel(modelBuilder);
        var model = modelBuilder.FinalizeModel();
        return CreateStateManager(model);
    }

    private static IStateManager CreateStateManager(IModel model)
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        return serviceProvider.GetRequiredService<IStateManager>();
    }

    private static void BuildJobModel(ModelBuilder mb)
        => mb.Entity<Job>(b =>
        {
            b.ComplexProperty(e => e.Error, eb =>
            {
                eb.ComplexProperty(ne => ne.InnerError, neb => neb.ComplexProperty(dne => dne.InnerError));
                eb.ComplexCollection(ne => ne.InnerErrors, neb => neb.ComplexProperty(dne => dne.InnerError));
            });
            b.ComplexCollection(e => e.Errors, eb =>
            {
                eb.ComplexProperty(ne => ne.InnerError, neb => neb.ComplexProperty(dne => dne.InnerError));
                eb.ComplexCollection(ne => ne.InnerErrors, neb => neb.ComplexProperty(dne => dne.InnerError));
            });
        });

    private static Job GetToObjectResult(Job job, bool? useOriginalValues)
    {
        var stateManager = CreateStateManager(BuildJobModel);

        var internalEntry = stateManager.GetOrCreateEntry(job);
        internalEntry.SetEntityState(EntityState.Unchanged);
        var entry = new EntityEntry<Job>(internalEntry);
        var targetValues = useOriginalValues == null
                ? entry.CurrentValues.Clone()
                : useOriginalValues.Value
                    ? entry.OriginalValues
                    : entry.CurrentValues;

        return (Job)targetValues.ToObject();
    }

    private static Dictionary<string, object?> ToDictionary(object obj, ITypeBase structuralType)
    {
        var dict = new Dictionary<string, object?>();
        var complexPropertyNames = new HashSet<string>();

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            complexPropertyNames.Add(complexProperty.Name);
            var pi = obj.GetType().GetProperty(complexProperty.Name);
            if (pi == null)
            {
                continue;
            }

            var value = pi.GetValue(obj);
            if (value == null)
            {
                dict[complexProperty.Name] = null;
            }
            else if (complexProperty.IsCollection && value is IList list)
            {
                var items = new List<Dictionary<string, object?>?>();
                for (var i = 0; i < list.Count; i++)
                {
                    items.Add(list[i] == null ? null : ToDictionary(list[i]!, complexProperty.ComplexType));
                }

                dict[complexProperty.Name] = items;
            }
            else if (!complexProperty.IsCollection)
            {
                dict[complexProperty.Name] = ToDictionary(value, complexProperty.ComplexType);
            }
        }

        foreach (var pi in obj.GetType().GetProperties())
        {
            if (!complexPropertyNames.Contains(pi.Name) && !dict.ContainsKey(pi.Name))
            {
                dict[pi.Name] = pi.GetValue(obj);
            }
        }

        return dict;
    }

    #endregion

    #region Model

    private class Job
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public RootJobError? Error { get; set; }
        public List<RootJobError> Errors { get; set; } = null!;
    }

    // Change these to structs once #31411 is fixed
    private class RootJobError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public JobError? InnerError { get; set; }
        public List<JobError> InnerErrors { get; set; } = null!;
    }

    private class JobError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public LeafJobError? InnerError { get; set; }
        public List<LeafJobError> InnerErrors { get; set; } = null!;
    }

    private class LeafJobError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }

    private class SimpleEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public IEnumerable<RelatedEntity> RelatedEntities { get; set; } = null!;
    }

    private class RelatedEntity
    {
        public int Id { get; set; }

        public int? SimpleEntityId { get; set; }

        public SimpleEntity? SimpleEntity { get; set; }

        public string? Name { get; set; }
    }

    #endregion
}
