// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class ModelCodeGeneratorSelectorTest
{
    [ConditionalFact]
    public void Select_returns_last_service_for_language()
    {
        var expected = new TestModelCodeGenerator("C#");
        var selector = new ModelCodeGeneratorSelector(
            new[] { new TestModelCodeGenerator("C#"), expected });

        var result = selector.Select(
            new ModelCodeGenerationOptions { Language = "C#" });

        Assert.Same(expected, result);
    }

    [ConditionalFact]
    public void Select_throws_when_no_service_for_language()
    {
        var selector = new ModelCodeGeneratorSelector(
            new[] { new TestModelCodeGenerator("C#") });
        var options = new ModelCodeGenerationOptions { Language = "VB" };

        var ex = Assert.Throws<OperationException>(
            () => selector.Select(options));

        Assert.Equal(DesignStrings.NoLanguageService("VB", nameof(IModelCodeGenerator)), ex.Message);
    }

    [ConditionalFact]
    public void Select_returns_last_templated_service_with_templates()
    {
        var expected = new TestTemplatedModelGenerator(hasTemplates: true);
        var selector = new ModelCodeGeneratorSelector(
            new IModelCodeGenerator[]
            {
                new TestTemplatedModelGenerator(hasTemplates: true),
                expected,
                new TestTemplatedModelGenerator(hasTemplates: false),
                new TestModelCodeGenerator("C#")
            });

        var result = selector.Select(
            new ModelCodeGenerationOptions { Language = "C#", ProjectDir = Directory.GetCurrentDirectory() });

        Assert.Same(expected, result);
    }

    [ConditionalFact]
    public void Select_returns_last_service_for_language_when_no_templates()
    {
        var expected = new TestModelCodeGenerator("C#");
        var selector = new ModelCodeGeneratorSelector(
            new IModelCodeGenerator[] { new TestTemplatedModelGenerator(hasTemplates: false), new TestModelCodeGenerator("C#"), expected });

        var result = selector.Select(
            new ModelCodeGenerationOptions { Language = "C#" });

        Assert.Same(expected, result);
    }

    private class TestModelCodeGenerator(string language) : ModelCodeGenerator(new ModelCodeGeneratorDependencies())
    {
        public override string Language { get; } = language;

        public override ScaffoldedModel GenerateModel(IModel model, ModelCodeGenerationOptions options)
            => throw new NotImplementedException();
    }

    private class TestTemplatedModelGenerator(bool hasTemplates) : TemplatedModelGenerator(new ModelCodeGeneratorDependencies())
    {
        private readonly bool _hasTemplates = hasTemplates;

        public override ScaffoldedModel GenerateModel(IModel model, ModelCodeGenerationOptions options)
            => throw new NotImplementedException();

        public override bool HasTemplates(string projectDir)
            => _hasTemplates;
    }
}
