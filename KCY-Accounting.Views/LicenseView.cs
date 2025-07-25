using System.Net.NetworkInformation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using KCY_Accounting.Interfaces;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using KCY_Accounting.Core;

namespace KCY_Accounting.Views;

public class LicenseView : UserControl, IView
{
    public string Title => "KCY-Accounting - Lizenzprüfung";
    public event EventHandler<ViewType>? NavigationRequested;

    private TextBox _inputBox;
    private TextBlock _messageBox;
    private Button _checkButton;
    public void Init()
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 16,
            Margin = new Thickness(30)
        };

        // Kompakter Header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 4
        };

        var titleText = new TextBlock
        {
            Text = "KCY-Accounting",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 149, 237)), // CornflowerBlue
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var subtitleText = new TextBlock
        {
            Text = "Lizenzaktivierung",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 170)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(subtitleText);

        // Kompakte Card für Eingabe
        var inputCard = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), // Leicht transparent
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 100, 149, 237)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(20, 16),
            Width = 320
        };

        var inputPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        var instructionText = new TextBlock
        {
            Text = "Lizenzschlüssel eingeben:",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 210)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _inputBox = new TextBox
        {
            Width = 260,
            Height = 32,
            Watermark = "Lizenzschlüssel",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 13,
            Padding = new Thickness(10, 6),
            Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 250)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 149, 237)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4)
        };

        _checkButton = new Button
        {
            Content = "Lizenz prüfen",
            Width = 120,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            FontSize = 12,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(0)
        };

        // Kompakter Status-Bereich
        _messageBox = new TextBlock
        {
            Text = "",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.Medium,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 280,
            Margin = new Thickness(0, 4, 0, 0)
        };

        inputPanel.Children.Add(instructionText);
        inputPanel.Children.Add(_inputBox);
        inputPanel.Children.Add(_checkButton);
        inputPanel.Children.Add(_messageBox);

        inputCard.Child = inputPanel;

        mainPanel.Children.Add(headerPanel);
        mainPanel.Children.Add(inputCard);

        _checkButton.Click += OnCheckButtonClickAsync;

        _checkButton.PointerEntered += OnCheckButtonPointerEntered;
        _checkButton.PointerExited += OnCheckButtonPointerExited;

        _inputBox.GotFocus += OnInputBoxGotFocus;
        _inputBox.LostFocus += OnInputBoxLostFocus;


        Content = mainPanel;
    }
    private void PointerEffect(bool? entered)
    {
        if (!_checkButton.IsEnabled) return;
        _inputBox.Background = entered == true ? _checkButton.Background = new SolidColorBrush(Color.FromRgb(120, 169, 255)) : _checkButton.Background = new SolidColorBrush(Color.FromRgb(100, 149, 237));
    }
    
    private void FocusEffect(bool? focus)
    {
        _inputBox.BorderBrush = focus == true ? new SolidColorBrush(Color.FromRgb(100, 149, 237)) : new SolidColorBrush(Color.FromArgb(100, 100, 149, 237));
    }
    private async Task CheckLicenseAsync(TextBox inputBox, TextBlock messageBox, Button button)
    {
        button.IsEnabled = false;
        button.Content = "Prüfung...";
        button.Background = new SolidColorBrush(Color.FromRgb(80, 80, 90));
        
        messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Gold
        messageBox.Text = "Wird überprüft...";

        try
        {
            var key = inputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 99, 99)); // Hell-Rot
                messageBox.Text = "Lizenzschlüssel eingeben";
                return;
            }

            var mcAddress = GetMacAddress();
            var isValid = await Client.IsValidLicenseAsync(key, mcAddress);
            if (isValid)
            {
                messageBox.Foreground = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // Hell-Grün
                messageBox.Text = "Lizenz gültig!";

                await Config.UpdateMcAddressAsync(mcAddress);
                await Config.UpdateLicenseKeyAsync(key);
                await Config.UpdateUserNameAsync();
                
                await Task.Delay(500);
                NavigationRequested?.Invoke(this, ViewType.ToS);
            }
            else
            {
                messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 99, 99));
                messageBox.Text = "Ungültiger Schlüssel";
            }
        }
        catch (ClientException ex)
        {
            messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 99, 99));
            messageBox.Text = $"Fehler: {ex.Message}";
        }
        catch (TimeoutException)
        {
            messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)); // Orange
            messageBox.Text = "Zeitüberschreitung - erneut versuchen";
        }
        catch (Exception ex)
        {
            messageBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 99, 99));
            messageBox.Text = $"Fehler: {ex.Message}";
        }
        finally
        {
            button.IsEnabled = true;
            button.Content = "Lizenz prüfen";
            button.Background = new SolidColorBrush(Color.FromRgb(100, 149, 237));
        }
    }
    private static string GetMacAddress()
    {
        var macBytes = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic =>
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                nic.GetPhysicalAddress().GetAddressBytes().Length == 6)
            .OrderByDescending(nic => nic.OperationalStatus == OperationalStatus.Up) // bevorzuge aktive
            .Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
            .FirstOrDefault();

        if (macBytes == null || macBytes.All(b => b == 0))
            return "00:00:00:00:00:00"; // fallback, wird nie null sein

        return string.Join(":", macBytes.Select(b => b.ToString("X2")));
    }
    
    private async void OnCheckButtonClickAsync(object? sender, RoutedEventArgs e)
    {
        await CheckLicenseAsync(_inputBox, _messageBox, _checkButton);
    }
    private void OnCheckButtonPointerEntered(object? sender, RoutedEventArgs e)
    {
        PointerEffect(true);
    }

    private void OnCheckButtonPointerExited(object? sender, RoutedEventArgs e)
    {
        PointerEffect(false);
    }
    private void OnInputBoxGotFocus(object? sender, RoutedEventArgs e)
    {
        FocusEffect(true);
    }

    private void OnInputBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        FocusEffect(false);
    }
    public void Dispose()
    {
        NavigationRequested = null;

        _inputBox.GotFocus -= OnInputBoxGotFocus;
        _inputBox.LostFocus -= OnInputBoxLostFocus;

        _checkButton.Click -= OnCheckButtonClickAsync;
        _checkButton.PointerEntered -= OnCheckButtonPointerEntered;
        _checkButton.PointerExited -= OnCheckButtonPointerExited;

        _inputBox = null!;
        _messageBox = null!;
        _checkButton = null!;

        (Content as Panel)?.Children.Clear();
        Content = null;
        
        Logger.Log("LicenseView disposed.");
    }

}