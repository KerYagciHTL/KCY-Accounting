using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KCY_Accounting.Core.ViewModels;

namespace KCY_Accounting.UI.Views;

public partial class TransportOrderEditView : UserControl
{
    public TransportOrderEditView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Wires the UploadDocumentCommand to an actual file picker dialog.
    /// The ViewModel receives the selected file path as a string parameter.
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is TransportOrderEditViewModel vm)
        {
            // Replace the placeholder command with a view-level handler
            // that opens a native file dialog and passes the path to the VM.
            vm.RequestFileUpload = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        AllowMultiple = false,
                        Title = "Dokument auswählen"
                    });

                if (files.Count > 0)
                {
                    var path = files[0].TryGetLocalPath();
                    if (path != null)
                        await vm.UploadDocumentAsync(path);
                }
            };
        }
    }
}

