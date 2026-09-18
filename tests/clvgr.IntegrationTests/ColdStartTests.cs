using clvgr.IntegrationTests.Base;
using DotNet.Testcontainers.Builders;

namespace clvgr.IntegrationTests;

public class ColdStartTests : TestsBase
{
    private string _scriptsPath;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _scriptsPath = Path.GetFullPath(Path.Combine(SolutionDir.DirectoryPath, "tests", "clvgr.IntegrationTests", "Scripts"));
    }

    [TestCase("enter-file-name_leave_no-file", false)]
    [TestCase("enter-file-name-password_leave_no-file", false)]
    [TestCase("enter-file-name-password_save_file-created", true)]
    public async Task ColdStartTest(string scriptName, bool expectFile)
    {
        await using var container = CreateContainerBuilder()
            .WithBindMount(_scriptsPath, "/tests")
            .WithCommand($"/tests/ColdStart/{scriptName}.sh")
            .Build();

        await container.StartAsync();

        await VerifyExitCode(container);

        if (expectFile)
        {
            byte[] f = await container.ReadFileAsync("/my/dir/secrets.clvgr");
            Assert.That(f.Length, Is.EqualTo(16 + 12 + 16 /*Salt + Nonce + Tag*/));
        }
        else
        {
            Assert.ThrowsAsync<FileNotFoundException>(async () => await container.ReadFileAsync("/my/dir/secrets.clvgr"));
        }
    }
}
