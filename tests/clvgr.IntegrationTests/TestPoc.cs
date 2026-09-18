using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace clvgr.IntegrationTests;

public class TestPoc
{
    private IContainer _container;

    private string _scriptsPath;

    [OneTimeSetUp]
    public async Task ImageSetup()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
#endif

        _scriptsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./Scripts"));
    }

    [SetUp]
    public void ContainerSetup()
    {
        _container = new ContainerBuilder("clvgr-tui-test")
            .WithBindMount(_scriptsPath, "/tests")
            .WithCommand("/tests/poc-test.sh")
            .Build();
    }

    [Test]
    public async Task Test1()
    {
        await _container.StartAsync();

        long exitCode = await _container.GetExitCodeAsync();

        if (exitCode != 0)
        {
            var logs = await _container.GetLogsAsync();
            Assert.Fail($"TUI Test failed with exit code {exitCode}.\nStdout/Stderr:\n{logs.Stdout}\n{logs.Stderr}");
        }

        Assert.Pass();
    }

    [TearDown]
    public async Task ContainerTeardown()
    {
        await _container.DisposeAsync();
    }
}
