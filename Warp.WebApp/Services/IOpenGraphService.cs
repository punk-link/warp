using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Services;

public interface IOpenGraphService
{
    EntryOpenGraphDescription BuildDescription(Entry entry);
    EntryOpenGraphDescription GetDefaultDescription();
}