using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using KCY_Accounting.Logic;

namespace KCY_Accounting.Views;

public class ToSView : UserControl, IView
{
    public event EventHandler<ViewType>? NavigationRequested;
    public string Title => "KCY-Accounting - Allgemeine Geschäftsbedingungen";

    private Button _acceptButton;
    private Button _declineButton;
    public Button AcceptButton => _acceptButton;
    public Button DeclineButton => _declineButton;

    public void Init()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        // Main container with gradient background matching the theme
        var mainGrid = new Grid
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromRgb(18, 18, 18), 0),
                    new GradientStop(Color.FromRgb(25, 25, 35), 1)
                ]
            },
            Margin = new Thickness(0)
        };

        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));  // Content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Buttons

        // Header Section
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 15,
            Margin = new Thickness(40, 40, 40, 30)
        };

        var titleText = new TextBlock
        {
            Text = "Allgemeine Geschäftsbedingungen",
            FontSize = 32,
            FontWeight = FontWeight.DemiBold,
            Foreground = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromRgb(120, 160, 240), 0),
                    new GradientStop(Color.FromRgb(100, 140, 220), 0.3),
                    new GradientStop(Color.FromRgb(140, 100, 200), 0.7),
                    new GradientStop(Color.FromRgb(120, 160, 240), 1)
                ]
            },
            HorizontalAlignment = HorizontalAlignment.Center,

        };

        var subtitleText = new TextBlock
        {
            Text = "KCY-Accounting Software",
            FontSize = 16,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 165, 180)),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontStyle = FontStyle.Italic,

        };

        // Elegante Separator Line
        var separatorLine = new Rectangle
        {
            Width = 400,
            Height = 2,
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromArgb(0, 120, 160, 240), 0),
                    new GradientStop(Color.FromRgb(120, 160, 240), 0.3),
                    new GradientStop(Color.FromRgb(140, 100, 200), 0.7),
                    new GradientStop(Color.FromArgb(0, 120, 160, 240), 1)
                ]
            },
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };

        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(subtitleText);
        headerPanel.Children.Add(separatorLine);

        // Scrollable content with elegant styling
        var contentContainer = new Border
        {
            Margin = new Thickness(40, 0, 40, 30),
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 120, 160, 240)),
            BorderThickness = new Thickness(1)
        };

        var scrollViewer = new ScrollViewer
        {
            Padding = new Thickness(0),
            Background = Brushes.Transparent
        };

        var contentPanel = new StackPanel
        {
            Margin = new Thickness(35, 30, 35, 30),
            Spacing = 18
        };

        // Terms content with elegant styling
        AddTermsSection(contentPanel, "§ 1 Geltungsbereich", 
            "Diese Allgemeinen Geschäftsbedingungen (AGB) gelten für die Nutzung der KCY-Accounting Software. Mit der Installation und Nutzung der Software akzeptieren Sie diese Bedingungen vollständig.");

        AddTermsSection(contentPanel, "§ 2 Lizenzgewährung", 
            "Ihnen wird eine nicht-ausschließliche, nicht-übertragbare Lizenz zur Nutzung der KCY-Accounting Software gewährt. Die Software darf nur für den vorgesehenen Zweck der Buchhaltung und Finanzverwaltung verwendet werden.");

        AddTermsSection(contentPanel, "§ 3 Nutzungsbeschränkungen", 
            "Es ist untersagt, die Software zu dekompilieren, zu reverse-engineeren oder anderweitig zu modifizieren. Die Weitergabe der Software an Dritte ohne ausdrückliche Zustimmung ist nicht gestattet.");

        AddTermsSection(contentPanel, "§ 4 Datenschutz und Sicherheit", 
            "Ihre Daten werden lokal auf Ihrem System gespeichert. Wir verpflichten uns, Ihre Privatsphäre zu schützen und keine persönlichen Daten ohne Ihre Zustimmung zu sammeln oder zu übertragen.");

        AddTermsSection(contentPanel, "§ 5 Haftungsausschluss", 
            "Die Software wird 'wie besehen' bereitgestellt. Wir übernehmen keine Gewähr für die Fehlerfreiheit oder die ununterbrochene Verfügbarkeit der Software. Die Haftung ist auf Vorsatz und grobe Fahrlässigkeit beschränkt.");

        AddTermsSection(contentPanel, "§ 6 Updates und Support", 
            "Updates werden nach eigenem Ermessen bereitgestellt. Ein Anspruch auf Support oder Updates besteht nicht, kann aber im Rahmen separater Vereinbarungen gewährt werden.");

        AddTermsSection(contentPanel, "§ 7 Kündigung", 
            "Diese Lizenz kann jederzeit durch Deinstallation der Software beendet werden. Bei Verstößen gegen diese AGB kann die Lizenz fristlos entzogen werden.");

        AddTermsSection(contentPanel, "§ 8 Änderungen der AGB", 
            "Diese AGB können jederzeit geändert werden. Nutzer werden über wesentliche Änderungen informiert und haben die Möglichkeit, der Fortsetzung der Nutzung zu widersprechen.");

        AddTermsSection(contentPanel, "§ 9 Anwendbares Recht", 
            "Es gilt deutsches Recht unter Ausschluss des UN-Kaufrechts. Gerichtsstand ist der Sitz des Anbieters.");

        AddTermsSection(contentPanel, "§ 10 Salvatorische Klausel", 
            "Sollten einzelne Bestimmungen unwirksam sein, bleibt die Wirksamkeit der übrigen Bestimmungen unberührt.");

        // Date stamp
        var dateBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 120, 160, 240)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var dateText = new TextBlock
        {
            Text = "Stand: 23.07.2025",
            FontSize = 12,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 165, 180)),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontStyle = FontStyle.Italic
        };

        dateBorder.Child = dateText;
        contentPanel.Children.Add(dateBorder);

        scrollViewer.Content = contentPanel;
        contentContainer.Child = scrollViewer;

        // Button Panel with elegant styling
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 25,
            Margin = new Thickness(40, 0, 40, 40)
        };

        _declineButton = CreateStyledButton("Ablehnen", Color.FromRgb(220, 80, 80), Color.FromRgb(180, 60, 60));
        _acceptButton = CreateStyledButton("Akzeptieren", Color.FromRgb(80, 180, 120), Color.FromRgb(60, 150, 100));

        _acceptButton.Click += AcceptButton_Click;
        _declineButton.Click += DeclineButton_Click;
        
        buttonPanel.Children.Add(_declineButton);
        buttonPanel.Children.Add(_acceptButton);

        // Add all elements to grid
        Grid.SetRow(headerPanel, 0);
        Grid.SetRow(contentContainer, 1);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerPanel);
        mainGrid.Children.Add(contentContainer);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }

    private void AddTermsSection(StackPanel parent, string title, string content)
    {
        var sectionPanel = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(120, 160, 240)),

        };

        var contentText = new TextBlock
        {
            Text = content,
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 230)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20,

        };

        sectionPanel.Children.Add(titleText);
        sectionPanel.Children.Add(contentText);
        parent.Children.Add(sectionPanel);
    }

    private static Button CreateStyledButton(string text, Color normalColor, Color hoverColor)
    {
        var button = new Button
        {
            Content = text,
            Width = 140,
            Height = 45,
            FontSize = 15,
            FontWeight = FontWeight.Medium,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(normalColor),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var hoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover"));
        hoverStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(hoverColor)));
        
        button.Styles.Add(hoverStyle);

        return button;
    }
    private async void AcceptButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Config.UpdateShowedAgbsAsync(true);
            NavigationRequested?.Invoke(this, ViewType.Welcome);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", ex.Message);
            Logger.Error(ex.Message);
        }
    }

    private async void DeclineButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var result = await MessageBox.ShowYesNo("Achtung", "Unsere Geschäftsbedigungen müssen für diese App akzeptiert werden \n Zurück?");
            if(!result) NavigationRequested?.Invoke(this, ViewType.None);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", ex.Message);
            Logger.Error(ex.Message);
        }
    }

    public void Dispose()
    {
        _acceptButton.Click -= AcceptButton_Click;
        _declineButton.Click -= DeclineButton_Click;

        _acceptButton = null!;
        _declineButton = null!;
        
        (Content as Grid)?.Children.Clear();
        Content = null;
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Logger.Log("ToSView disposed.");
    }
}