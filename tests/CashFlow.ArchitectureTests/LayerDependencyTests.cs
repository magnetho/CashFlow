using CashFlow.Domain.Entities;
using NetArchTest.Rules;

namespace CashFlow.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Theory]
    [InlineData("CashFlow.Application")]
    [InlineData("CashFlow.Infrastructure")]
    [InlineData("CashFlow.Api")]
    [InlineData("CashFlow.Worker")]
    public void Domain_ShouldNotDependOnOuterLayers(string forbiddenNamespace)
    {
        var result = Types.InAssembly(typeof(CashEntry).Assembly)
            .ShouldNot().HaveDependencyOn(forbiddenNamespace).GetResult();
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void DomainEntities_ShouldNotExposePublicSetters()
    {
        var publicSetters = typeof(CashEntry).GetProperties()
            .Where(property => property.SetMethod?.IsPublic == true)
            .Select(property => property.Name);
        Assert.Empty(publicSetters);
    }
}
