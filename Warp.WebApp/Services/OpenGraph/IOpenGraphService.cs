using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Services.OpenGraph;

public interface IOpenGraphService
{
    EntryOpenGraphDescription BuildDescription(Entry entry);
    EntryOpenGraphDescription GetDefaultDescription();
}