using Siener.Models;

namespace Siener.Services;

public interface ISharedDataService
{
    public List<Camera>? Cameras { get; set; }
    public List<PushSubscription>? PushSubscriptions { get; set; }

}

public class SharedDataService : ISharedDataService
{
    public List<Camera>? Cameras { get; set; }
    public List<PushSubscription>? PushSubscriptions { get; set; }


    public SharedDataService() {}
}
