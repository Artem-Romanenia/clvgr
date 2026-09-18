using clvgr.IntegrationTests.Base;
using DotNet.Testcontainers.Builders;

namespace clvgr.IntegrationTests;

public class EmptyFileTests : TestsBase
{
    private string _scriptsPath;
    private string _filesPath;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _scriptsPath = Path.GetFullPath(Path.Combine(SolutionDir.DirectoryPath, "tests", "clvgr.IntegrationTests", "Scripts"));
        _filesPath = Path.GetFullPath(Path.Combine(SolutionDir.DirectoryPath, "tests", "clvgr.IntegrationTests", "Files"));
    }

    [TestCase("select-file-add-resource-cancel_leave_file-unchanged")]
    [TestCase("select-file-add-resource-cancel_leave_file-unchanged")]
    public async Task ColdStartTest(string scriptName)
    {
        await using var container = CreateContainerBuilder()
            .WithBindMount(_scriptsPath, "/tests")
            .WithBindMount(_filesPath, "/files")
            .WithCommand($"/tests/EmptyFile/{scriptName}.sh")
            .Build();

        await container.StartAsync();

        await VerifyExitCode(container);

        byte[] f = await container.ReadFileAsync("/files/empty.clvgr");
        Assert.That(f.Length, Is.EqualTo(16 + 12 + 16 /*Salt + Nonce + Tag*/));
    }
}
