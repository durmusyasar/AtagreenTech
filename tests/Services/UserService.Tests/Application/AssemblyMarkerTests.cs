using AtaGreenTech.UserService.Application;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AssemblyMarkerTests
{
    [Fact]
    public void Application_assembly_is_discoverable_for_omnisync_scanning()
    {
        Assert.Equal("AtaGreenTech.UserService.Application", typeof(AssemblyMarker).Assembly.GetName().Name);
    }
}
