using Microsoft.FeatureManagement;

namespace Warp.WebApp.Extensions;

public static class FeatureManagerExtensions
{
    public static bool IsEnabled(this IFeatureManager featureManager, string featureName)
    {
        ArgumentNullException.ThrowIfNull(featureManager, nameof(featureManager));
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName, nameof(featureName));

        return featureManager.IsEnabledAsync(featureName).GetAwaiter().GetResult();
    }
}
