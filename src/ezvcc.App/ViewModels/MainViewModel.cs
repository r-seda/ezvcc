using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ezvcc.App.Audio;

namespace Ezvcc.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly AudioEngine _engine = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool isRunning;

    [ObservableProperty]
    private double inputLevel;

    [ObservableProperty]
    private string statusText = "停止中";

    public MainViewModel()
    {
        _engine.LevelUpdated += OnLevelUpdated;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        try
        {
            _engine.Start();
            IsRunning = true;
            StatusText = "稼働中";
        }
        catch (Exception ex)
        {
            StatusText = $"開始失敗: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _engine.Stop();
        IsRunning = false;
        InputLevel = 0;
        StatusText = "停止中";
    }

    private bool CanStart() => !IsRunning;

    private bool CanStop() => IsRunning;

    private void OnLevelUpdated(object? sender, AudioLevelEventArgs e)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            InputLevel = e.Peak;
        }
        else
        {
            dispatcher.BeginInvoke(() => InputLevel = e.Peak);
        }
    }

    public void Dispose()
    {
        _engine.LevelUpdated -= OnLevelUpdated;
        _engine.Dispose();
    }
}
