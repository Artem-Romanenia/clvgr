using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using static clvgr.Core.Clipboard;
using static clvgr.Core.Encryption;

namespace clvgr.Controls;

internal class ResourceList : View
{
    private readonly IClipboardAccessor _clipboardAccessor;
    private readonly ICryptor _cryptor;
    private readonly ILogger<ResourceList> _logger;
    private readonly ObservableCollection<Resource> _resources;

    public ResourceList(IClipboardAccessor clipboardAccessor, ILogger<ResourceList> logger, SecretsFileManager secretsFileManager)
    {
        _clipboardAccessor = clipboardAccessor;
        _cryptor = secretsFileManager.Cryptor;
        _logger = logger;
        _resources = secretsFileManager.Resources;

        CanFocus = true;
        TabStop = TabBehavior.TabGroup;

        _resources.CollectionChanged += OnResourcesChanged;

        AddCommand(Command.Copy, CopyToClipboard);
        AddCommand(Command.Open, Edit);
        AddCommand(Command.New, Add);
        AddCommand(Command.Cut, Delete);

        KeyBindings.Clear();

        KeyBindings.Add(Key.Space, Command.Copy);
        KeyBindings.Add(Key.Enter, Command.Open);
        KeyBindings.Add(Key.InsertChar, Command.New);
        KeyBindings.Add(Key.DeleteChar, Command.Cut);

        ShowResources();
    }

    public ResourceListItem? SelectedItem => SubViews.FirstOrDefault(v => v.HasFocus) as ResourceListItem;

    private bool? CopyToClipboard(ICommandContext? ctx)
    {
        ResourceListItem? selectedItem;
        if ((selectedItem = SelectedItem) is null) return null;

        try
        {
            _cryptor.Decrypt(selectedItem.Resource.Secret.ToArray(), _clipboardAccessor.PutToClipboard);
            using var p = new AutoclosePopup("Secret was copied to clipboard.");

            this.PresentApp.Run(p);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnhandledError");
            MessageBox.ErrorQuery(this.PresentApp, "Error", "Error while copying a secret to a clipboard.");
        }

        return true;
    }

    private bool? Add(ICommandContext? ctx)
    {
        using ResourceDialog formWindow = new(_cryptor, null)
        {
            Width = Dim.Percent(70),
            Height = Dim.Auto(),
            X = Pos.Center(),
            Y = Pos.Center()
        };

        this.PresentApp.Run(formWindow);

        if (formWindow.Result is Resource editedItem)
        {
            _resources.Add(editedItem);
        }

        return true;
    }

    private bool? Edit(ICommandContext? ctx)
    {
        ResourceListItem? selectedItem;
        if ((selectedItem = SelectedItem) is null) return null;

        using ResourceDialog formWindow = new(_cryptor, selectedItem.Resource)
        {
            Width = Dim.Percent(70),
            Height = Dim.Auto(),
            X = Pos.Center(),
            Y = Pos.Center()
        };

        this.PresentApp.Run(formWindow);

        if (formWindow.Result is Resource editedItem)
        {
            _resources.Remove(selectedItem.Resource);
            _resources.Add(editedItem);
        }

        return true;
    }

    private bool? Delete(ICommandContext? ctx)
    {
        ResourceListItem? selectedItem;
        if ((selectedItem = SelectedItem) is null) return null;

        _resources.Remove(selectedItem.Resource);

        return true;
    }

    private void OnResourcesChanged(object? sender, NotifyCollectionChangedEventArgs args) => ShowResources();

    private void ShowResources()
    {
        RemoveAll();

        ResourceListItem? previousItem = null;
        foreach (var resource in _resources)
        {
            var resourceItem = new ResourceListItem(resource)
            {
                X = Pos.Center(),
                Y = previousItem is { } ? Pos.Bottom(previousItem) : 0,
                Width = Dim.Fill()
            };

            Add(resourceItem);

            previousItem = resourceItem;
        }

        SubViews.FirstOrDefault()?.SetFocus();

    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _resources.CollectionChanged -= OnResourcesChanged;
        }

        base.Dispose(disposing);
    }
}
