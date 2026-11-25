using Avalonia.Controls;
using Capstone.Core.Storage;
using Capstone.Core.ViewModels;

namespace Capstone.Desktop;

/// <summary>
/// Loads the library, hands it to the view model, saves it on close. Everything else is bindings.
/// </summary>
public partial class MainWindow : Window
{
    private readonly string _savePath = LibraryFile.DefaultPath;
    private Library? _library;
    private CatalogueViewModel? _viewModel;
    private bool _saved;

    public MainWindow()
    {
        InitializeComponent();
        Opened += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _library = await LibraryFile.LoadAsync(_savePath);   // awaited: the window keeps drawing meanwhile
            if (_library.Count == 0)
                SampleLibrary.AddTo(_library, Path.Combine(AppContext.BaseDirectory, "data", "library.json"));
        }
        catch (InvalidDataException e)
        {
            // Corrupt, not missing: say so, and never save over it.
            Status.Text = $"Couldn't read {_savePath}:\n{e.Message}";
            return;
        }

        _viewModel = new CatalogueViewModel(_library);
        DataContext = _viewModel;
        Status.IsVisible = false;
        Main.IsVisible = true;
    }

    /// <summary>
    /// Closing can't wait for an await, so: cancel this close, save, then close again for real.
    /// </summary>
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_saved || _library is null) return;

        e.Cancel = true;
        _saved = true;
        await LibraryFile.SaveAsync(_library, _savePath);
        _viewModel?.Dispose();
        Close();
    }
}
