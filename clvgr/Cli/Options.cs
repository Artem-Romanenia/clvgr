using CommandLine;

namespace clvgr.Cli;

[Verb("run", isDefault: true, HelpText = "Run terminal interface.")]
public class Options
{
    [Option('p', "path", Required = false, HelpText = "Path to a secrets file.")]
    public string? Path { get; set; }
}

[Verb("clip", HelpText = "Copy secret of a specified resource to clipboard.")]
public class ClipOptions()
{
    [Value(0, Required = true, HelpText = "Resource Short Name.")]
    public required string ShortName { get; set; }

    [Option('f', "field", Required = false, HelpText = "Name of the additional field to copy instead of a main secret.")]
    public string? Field { get; set; }

    [Option('p', "path", Required = false, HelpText = "Path to a secrets file.")]
    public string? Path { get; set; }
}

[Verb("print", HelpText = "Print resource information.")]
public class PrintOptions()
{
    [Value(0, Required = true, HelpText = "Resource Short Name.")]
    public required string ShortName { get; set; }

    [Option('a', "all", Required = false, HelpText = "Print ALL data, including secrets.", SetName = "data-to-print")]
    public bool All { get; set; }

    [Option('s', "secret", Required = false, HelpText = "Print just secret.", SetName = "data-to-print")]
    public bool Secret { get; set; }

    [Option('f', "field", Required = false, HelpText = "Name of the additional field to copy instead of a main secret.")]
    public string? Field { get; set; }

    [Option('p', "path", Required = false, HelpText = "Path to a secrets file.")]
    public string? Path { get; set; }
}
