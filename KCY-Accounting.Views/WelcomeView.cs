using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using KCY_Accounting.Interfaces;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using Avalonia.Threading;
using KCY_Accounting.Core;
using KCY_Accounting.Logic;

namespace KCY_Accounting.Views;
public class WelcomeView : UserControl, IView
{
    public string Title => "KCY-Accounting - Willkommen";
    public WindowIcon Icon => new("resources/pictures/welcome.ico");

    public event EventHandler<ViewType>? NavigationRequested;

    private bool _animationsStarted;
    private readonly string[] _loadingTexts = File.ReadAllLines("resources/appdata/loading-texts.txt");
    private int _currentTextIndex;
    private DispatcherTimer? _separatorTimer;
    private DispatcherTimer? _textTimer;
    private DispatcherTimer? _dotTimer;
    public void Init()
    {
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromRgb(15, 15, 20), 0),
                    new GradientStop(Color.FromRgb(22, 22, 28), 0.6),
                    new GradientStop(Color.FromRgb(18, 18, 25), 1)
                ]
            }
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 60
        };

        // Logo/Title Section - starts invisible and slides in
        var logoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 20,
            Opacity = 0,
            RenderTransform = new TranslateTransform(0, -100)
        };

        var mainTitle = new TextBlock
        {
            Text = Config.UserName,
            FontSize = 56,
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
            LetterSpacing = 1.5
        };

        string[] subtitles = 
        [
            "Buchhaltung, die einfach funktioniert. KCY-Accounting.",
            "Deine Aufträge. Sicher. Übersichtlich. Bereit. KCY-Accounting.",
            "Weniger Aufwand. Mehr Überblick. KCY-Accounting.",
            "Maximale Kontrolle. KCY-Accounting.",
            "Zahlen im Griff. Finanzen im Blick. KCY-Accounting."
        ];
        
        var slogan = subtitles[Random.Shared.Next(0, subtitles.Length)];
        var subtitle = new TextBlock
        {
            Text = slogan,
            FontSize = 18,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 165, 180)),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontStyle = FontStyle.Italic,
            LetterSpacing = 0.8
        };

        logoPanel.Children.Add(mainTitle);
        logoPanel.Children.Add(subtitle);

        // Elegante Separator Line
        var separatorLine = new Rectangle
        {
            Width = 450,
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
            Opacity = 0,
            RenderTransform = new ScaleTransform(0, 1)
        };

        // Status Section
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Opacity = 0,
            RenderTransform = new TranslateTransform(0, 50)
        };

        // Loading Text
        var loadingText = new TextBlock
        {
            Text = _loadingTexts[0],
            FontSize = 14,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(140, 145, 160)),
            HorizontalAlignment = HorizontalAlignment.Center,
            LetterSpacing = 0.5
        };

        var loadingIndicator = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8
        };

        var dot1 = CreateLoadingDot();
        var dot2 = CreateLoadingDot();
        var dot3 = CreateLoadingDot();

        loadingIndicator.Children.Add(dot1);
        loadingIndicator.Children.Add(dot2);
        loadingIndicator.Children.Add(dot3);

        statusPanel.Children.Add(loadingText);
        statusPanel.Children.Add(loadingIndicator);

        mainPanel.Children.Add(logoPanel);
        mainPanel.Children.Add(separatorLine);
        mainPanel.Children.Add(statusPanel);

        mainBorder.Child = mainPanel;
        Content = mainBorder;

        Loaded += async (_, _) => await StartChillAnimationSequenceAsync(logoPanel, separatorLine, statusPanel, loadingText, dot1, dot2, dot3);
    }

    private Border CreateLoadingDot()
    {
        return new Border
        {
            Width = 6,
            Height = 6,
            Background = new SolidColorBrush(Color.FromRgb(120, 160, 240)),
            CornerRadius = new CornerRadius(3),
            Opacity = 0.3
        };
    }

    private async Task StartChillAnimationSequenceAsync(Panel logoPanel, Rectangle separatorLine, Panel statusPanel, TextBlock loadingText, Border dot1, Border dot2, Border dot3)
    {
        if (_animationsStarted) return;
        _animationsStarted = true;

        await AnimateElementSmoothly(logoPanel, 
            opacity: 1, 
            translateY: 0, 
            duration: 1500, 
            easing: new CubicEaseOut());

        await AnimateElementScale(separatorLine, 
            opacity: 1, 
            scaleX: 1, 
            duration: 800, 
            easing: new CubicEaseOut());

        await AnimateElementSmoothly(statusPanel, 
            opacity: 1, 
            translateY: 0, 
            duration: 1000, 
            easing: new CubicEaseOut());

        StartChillContinuousAnimations(separatorLine, loadingText, dot1, dot2, dot3);

        await Task.Delay(5000);
        NavigationRequested?.Invoke(this, ViewType.Main);
    }

    private async Task AnimateElementSmoothly(Control element, double? opacity = null, double? translateX = null, double? translateY = null, int duration = 1000, IEasing? easing = null)
    {
        easing ??= new CubicEaseOut();

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            Easing = (Easing)easing,
            FillMode = FillMode.Forward
        };

        if (opacity.HasValue)
        {
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(0.0), 
                Setters = { new Setter(OpacityProperty, element.Opacity) }
            });
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(1.0), 
                Setters = { new Setter(OpacityProperty, opacity.Value) }
            });
        }

        if (element.RenderTransform is TranslateTransform transform)
        {
            if (translateX.HasValue)
            {
                animation.Children.Add(new KeyFrame 
                { 
                    Cue = new Cue(0.0), 
                    Setters = { new Setter { Property = TranslateTransform.XProperty, Value = transform.X } }
                });
                animation.Children.Add(new KeyFrame 
                { 
                    Cue = new Cue(1.0), 
                    Setters = { new Setter { Property = TranslateTransform.XProperty, Value = translateX.Value } }
                });
            }

            if (translateY.HasValue)
            {
                animation.Children.Add(new KeyFrame 
                { 
                    Cue = new Cue(0.0), 
                    Setters = { new Setter { Property = TranslateTransform.YProperty, Value = transform.Y } }
                });
                animation.Children.Add(new KeyFrame 
                { 
                    Cue = new Cue(1.0), 
                    Setters = { new Setter { Property = TranslateTransform.YProperty, Value = translateY.Value } }
                });
            }
        }

        if (animation.Children.Count > 0)
        {
            await animation.RunAsync(element);
        }
    }

    private async Task AnimateElementScale(Control element, double? opacity = null, double? scaleX = null, int duration = 1000, IEasing? easing = null)
    {
        easing ??= new CubicEaseOut();

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(duration),
            Easing = (Easing)easing,
            FillMode = FillMode.Forward
        };

        if (opacity.HasValue)
        {
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(0.0), 
                Setters = { new Setter(OpacityProperty, element.Opacity) }
            });
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(1.0), 
                Setters = { new Setter(OpacityProperty, opacity.Value) }
            });
        }

        if (element.RenderTransform is ScaleTransform transform && scaleX.HasValue)
        {
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(0.0), 
                Setters = { new Setter { Property = ScaleTransform.ScaleXProperty, Value = transform.ScaleX } }
            });
            animation.Children.Add(new KeyFrame 
            { 
                Cue = new Cue(1.0), 
                Setters = { new Setter { Property = ScaleTransform.ScaleXProperty, Value = scaleX.Value } }
            });
        }

        if (animation.Children.Count > 0)
        {
            await animation.RunAsync(element);
        }
    }

    private void StartChillContinuousAnimations(Rectangle separatorLine, TextBlock loadingText, Border dot1, Border dot2, Border dot3)
    {
        _separatorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _separatorTimer.Tick += async (_, _) =>
        {
            var pulseAnimation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(2000),
                Easing = new SineEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 1.0) } },
                    new KeyFrame { Cue = new Cue(0.5), Setters = { new Setter(OpacityProperty, 0.4) } },
                    new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 1.0) } }
                }
            };
            await pulseAnimation.RunAsync(separatorLine);
        };
        _separatorTimer.Start();

        _textTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _textTimer.Tick += async (_, _) =>
        {
            var fadeOut = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(400),
                Easing = new CubicEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 0.0) } }
                }
            };
            await fadeOut.RunAsync(loadingText);

            _currentTextIndex = (_currentTextIndex + 1) % _loadingTexts.Length;
            loadingText.Text = _loadingTexts[_currentTextIndex];

            var fadeIn = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(600),
                Easing = new CubicEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 1.0) } }
                }
            };
            await fadeIn.RunAsync(loadingText);
        };
        _textTimer.Start();

        StartDotWaveAnimation(dot1, dot2, dot3);
    }

    private void StartDotWaveAnimation(Border dot1, Border dot2, Border dot3)
    {
        _dotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        var phase = 0;
        
        _dotTimer.Tick += async (_, _) =>
        {
            int[] delays = [phase * 200, (phase + 1) * 200, (phase + 2) * 200];
            var delay = delays[phase];
            
            AnimateDot(dot1, delay);
            await Task.Delay(200);
            AnimateDot(dot2, 0);
            await Task.Delay(200);  
            AnimateDot(dot3, 0);

            phase = (phase + 1) % 3;
        };
        _dotTimer.Start();
    }

    private static async void AnimateDot(Border dot, int delay)
    {
        try
        {
            if (delay > 0) await Task.Delay(delay);

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(600),
                Easing = new SineEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 0.3) } },
                    new KeyFrame { Cue = new Cue(0.5), Setters = { new Setter(OpacityProperty, 1.0) } },
                    new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 0.3) } }
                }
            };
            await animation.RunAsync(dot);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", ex.Message);
            Logger.Error("Fehler bei der Animation des Punktes: " + ex.Message);
        }
    }

    public void Dispose()
    {
        NavigationRequested = null;
        
        if (_separatorTimer != null)
        {
            _separatorTimer.Stop();
            _separatorTimer = null;
        }

        if (_textTimer != null)
        {
            _textTimer.Stop();
            _textTimer = null;
        }

        if (_dotTimer != null)
        {
            _dotTimer.Stop();
            _dotTimer = null;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Logger.Log("WelcomeView disposed.");
    }
}