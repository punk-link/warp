using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Services.OpenGraph;

public interface IOpenGraphService
{
    EntryOpenGraphDescription BuildDescription(Guid entryInfoId, Entry entry);
    EntryOpenGraphDescription GetDefaultDescription();
}