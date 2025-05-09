using Microsoft.FeatureManagement;
using NSubstitute;

namespace Warp.WebApp.Tests.Infrastructure;

/// <summary>
/// Custom extension methods for feature manager testing
/// </summary>
public static class TestFeatureManagerExtensions
{
    /// <summary>
    /// Creates a feature manager that will return true for IsEnabled for the specific feature
    /// </summary>
    public static IFeatureManager ConfigureFeature(this IFeatureManager featureManager, string featureName, bool isEnabled)
    {
        var taskResult = Task.FromResult(isEnabled);
        
        var customFeatureManager = Substitute.For<IFeatureManager>();
        
        customFeatureManager.IsEnabledAsync(Arg.Is<string>(name => name == featureName))
            .Returns(taskResult);
            
        customFeatureManager.IsEnabledAsync(Arg.Is<string>(name => name != featureName))
            .Returns(callInfo => featureManager.IsEnabledAsync(callInfo.Arg<string>()));
            
        customFeatureManager.When(fm => fm.GetFeatureNamesAsync())
            .Do(callInfo => featureManager.GetFeatureNamesAsync());
            
        return customFeatureManager;
    }
}
