using Microsoft.Extensions.Logging;

namespace clvgr.Controls;

internal class AppManager(
    SecretsFileManagerFactory secretsFileManagerFactory,
    SettingsManager settingsManager,
    ILogger<AppManager> logger,
    Func<SecretsFileManager, ResourceList> resourceListFactory,
    IApplication app,
    Window window)
{
    private MainContent? _mainContent;

    public void RunMainWindow()
    {
        MainMenu menu = new(this) { Height = Dim.Auto(), Width = Dim.Fill() };

        _mainContent = new()
        {
            Y = Pos.Bottom(menu),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        if (CurrentSecretsFileManager is { })
        {
            _mainContent.Show(resourceListFactory(CurrentSecretsFileManager));
        }
        else
        {
            _mainContent.Show(GetLogoView());
        }

        MainFooter footer = new();

        window.Add(menu, _mainContent, footer);
        window.IsRunningChanging += (_, e) =>
        {
            if (e.NewValue) return;
            bool closed = TryCloseCurrentSecretsFile();
            e.Cancel = !closed;
        };

        app.Run(window);
    }


    public SecretsFileManager? CurrentSecretsFileManager { get; private set; }

    public void SelectSecretsFile()
    {
        var result = RunSecretsFileDialog();

        if (result is [var path])
        {
            if (CurrentSecretsFileManager?.SecretsFileName == path)
            {
                app.ShowWarning("Warning", "File is already opened.");
                return;
            }

            InitializeSecretsFileManager(path);
        }
    }

    public void InitializeSecretsFileManager(string secretsFilePath)
    {
        try
        {
            if (!TryCloseCurrentSecretsFile()) return;

            var secretsFileManagerConstructor = secretsFileManagerFactory.Preconstruct(secretsFilePath);

            SecretsFileManager? secretsFileManager = null;
            using MasterPasswordReaderDialog passwordDialog = new(secretsFilePath, passwordBytes 
                => secretsFileManager = secretsFileManagerConstructor.Construct(passwordBytes));

            app.Run(passwordDialog);

            if (secretsFileManager is null)
            {
                return;
            }

            settingsManager.Apply(UserSettings.Defaults with { LastSecretsFile = secretsFilePath });

            CurrentSecretsFileManager = secretsFileManager;

            _mainContent?.Show(resourceListFactory(secretsFileManager));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error.");
            MessageBox.ErrorQuery(app, "Error", $"Unexpected error.");
        }
    }

    public void ShowPreferencesDialog()
    {
        using SettingsDialog settingsDialog = new(settingsManager)
        {
            X = Pos.Center(),
            Y = Pos.Center()
        };

        app.Run(settingsDialog);
    }

    public void ShowAboutDialog()
    {
        using AboutDialog aboutDialog = new(CurrentSecretsFileManager)
        {
            X = Pos.Center(),
            Y = Pos.Center()
        };

        app.Run(aboutDialog);
    }

    private IReadOnlyList<string>? RunSecretsFileDialog()
    {
        try
        {
            using FileDialog fileDialog = new()
            {
                Title = "Select Secrets file",
                Width = Dim.Percent(70),
                Height = Dim.Percent(80),
                X = Pos.Center(),
                Y = Pos.Center(),
                OpenMode = OpenMode.File,
                AllowsMultipleSelection = false,
                MustExist = false,
                AllowedTypes = [new AllowedType("Secrets file", ".clvgr")]
            };
            fileDialog.Style.TableStyle?.AlwaysUseNormalColorForVerticalCellLines = true;
            if (fileDialog.Style.TableStyle is { })
            {
                foreach (var colStyle in fileDialog.Style.TableStyle.ColumnStyles.Values)
                {
                    colStyle.ColorGetter = args => SchemeManager.TryGetScheme("TableCustom", out var scheme) ? scheme : null;
                }
            }

            app.Run(fileDialog);

            return fileDialog.Result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error.");
            app.ShowError("Error", $"Unexpected error.");
            return null;
        }
    }

    private bool TryCloseCurrentSecretsFile()
    {
        if (CurrentSecretsFileManager is { })
        {
            if (CurrentSecretsFileManager.IsModified())
            {
                int? res = MessageBox.Query(app, $"Unsaved changes ({Path.GetFileName(CurrentSecretsFileManager.SecretsFileName)})", "Secrets file has unsaved changes. Save?", ["Yes", "No", "Cancel"]);

                switch (res)
                {
                    case 0:
                        CurrentSecretsFileManager.Persist();
                        break;
                    case 1: break;
                    case 2: return false;
                }
            }

            CurrentSecretsFileManager.Dispose();
        }

        return true;
    }

    private static View GetLogoView()
    {
        var logoView = new View();
        var logo = new Logo() { X = Pos.Center(), Y = Pos.Center() };
        var label = new Label() { Text = "Type [Alt+F,S] to create/open secrets file.", X = Pos.Center(), Y = Pos.Bottom(logo) };
        logoView.Add(logo, label);

        return logoView;
    }
}
