using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Services.OpenGraph;

public interface IOpenGraphService
{
    EntryOpenGraphDescription BuildDescription(string descriptionSource, Uri? previewImageUrl);
    EntryOpenGraphDescription GetDefaultDescription();
}