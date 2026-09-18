using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace clvgr.IntegrationTests.Base;

public abstract class TestsBase
{
    [OneTimeSetUp]
    public async Task SetupBase()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
#endif

        SolutionDir = CommonDirectoryPath.GetSolutionDirectory();
    }

    protected CommonDirectoryPath SolutionDir { get; private set; }

    protected ContainerBuilder CreateContainerBuilder() => new("clvgr-tui-test");

    protected async Task VerifyExitCode(IContainer container)
    {
        long exitCode = await container.GetExitCodeAsync();

        if (exitCode != 0)
        {
            var (stdout, stderr) = await container.GetLogsAsync();
            Assert.Fail($"""
                TUI Test failed with exit code {exitCode}.
                Stdout/Stderr:
                {stdout}
                {stderr}
                """);
        }
    }
}
