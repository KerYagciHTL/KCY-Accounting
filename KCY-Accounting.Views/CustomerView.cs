using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;
using KCY_Accounting.Logic;

namespace KCY_Accounting.Views;

public class CustomerView : UserControl, IView
{
    public const string FILE_PATH = "resources/appdata/customers.kdb";
    
    public string Title => "KCY-Accounting - Kundenansicht";
    public WindowIcon Icon => new("resources/pictures/customer-management.ico");
    
    public event EventHandler<ViewType>? NavigationRequested;
    
    private static readonly Country[] EnumCountries = Enum.GetValues<Country>();
    private static readonly CountryCode[] EnumCountryCodes = Enum.GetValues<CountryCode>();
    private static readonly NetCalculationType[] EnumNetCalculationTypes = Enum.GetValues<NetCalculationType>();
    
    private ObservableCollection<Customer> _customers = [];
    private int _countOfCustomersOnLoad;
    
    private ListBox _customerListBox;
    private StackPanel _editPanel;
    private TextBox _customerNumberBox, _nameBox, _addressBox, _postalCodeBox, _cityBox, _uidNumberBox, _emailBox, _paymentDueBox;
    private ComboBox _countryCombo, _uidCountryCombo, _netCalculationCombo;
    private Button _saveButton, _deleteButton, _newButton, _cancelButton, _backButton;
    private bool _isEditing;
    private Customer? _selectedCustomer;

    public void Init()
    {
        LoadCustomerData();
        
        var mainGrid = new Grid
        {
            Margin = new Thickness(20)
        };
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Footer

        // Header mit Titel und Buttons
        var headerPanel = new Grid
        {
            Margin = new Thickness(0, 0, 0, 20)
        };
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

         _backButton = new Button
        {
            Content = "← Zurück",
            Classes = { "modern-button", "back-button" },
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(20, 10),
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 55)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 70)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };
        _backButton.Click += BackButton_Click;
        
        KeyDown += OnKeyDown;

        var titleText = new TextBlock
        {
            Text = "Kundenverwaltung",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var saveHint = new TextBlock
        {
            Text = "Strg + S zum Speichern",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(_backButton, 0);
        Grid.SetColumn(titleText, 1);
        Grid.SetColumn(saveHint, 2);
        headerPanel.Children.Add(_backButton);
        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(saveHint);

        // Hauptcontent Grid
        var contentGrid = new Grid();
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(2, GridUnitType.Star));
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

        // Linke Seite - Kundenliste
        var leftPanel = CreateCustomerListPanel();
        
        // Splitter
        var splitter = new GridSplitter
        {
            Width = 3,
            Background = new SolidColorBrush(Color.FromRgb(60, 60, 70)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Rechte Seite - Edit Panel
        _editPanel = CreateEditPanel();

        Grid.SetColumn(leftPanel, 0);
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(_editPanel, 2);
        contentGrid.Children.Add(leftPanel);
        contentGrid.Children.Add(splitter);
        contentGrid.Children.Add(_editPanel);

        // Footer mit Action Buttons
        var footerPanel = CreateFooterPanel();

        Grid.SetRow(headerPanel, 0);
        Grid.SetRow(contentGrid, 1);
        Grid.SetRow(footerPanel, 2);
        mainGrid.Children.Add(headerPanel);
        mainGrid.Children.Add(contentGrid);
        mainGrid.Children.Add(footerPanel);

        EnableForm(false);
        Content = mainGrid;
        
        Logger.Log($"UI loaded with {_customers.Count} customers.");
    }

    private Border CreateCustomerListPanel()
    {
        var panel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(50, 50, 60)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 10, 0)
        };

        // Header mit Kundenliste-Titel und Anzahl
        var headerPanel = new Grid();
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var listHeader = new TextBlock
        {
            Text = "Kundenliste",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        var customerCountText = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Binding für die Kundenanzahl
        customerCountText.Bind(TextBlock.TextProperty, 
            new Binding("Count") { Source = _customers, StringFormat = "{0} Kunden" });

        Grid.SetColumn(listHeader, 0);
        Grid.SetColumn(customerCountText, 1);
        headerPanel.Children.Add(listHeader);
        headerPanel.Children.Add(customerCountText);

        // ScrollViewer für die ListBox
        _customerListBox = new ListBox
        {
            Background = Brushes.Transparent,
            SelectionMode = SelectionMode.Single,
            ItemsSource = _customers
        };

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 15, 0, 0),
            Content = _customerListBox
        };

        // Custom ItemTemplate für die Kunden-Darstellung
        var itemTemplate = new FuncDataTemplate<Customer>((customer, _) =>
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if(customer == null) return null;

            var customerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10),
                Margin = new Thickness(0, 2)
            };

            var customerPanel = new StackPanel();

            // Erste Zeile: Kundennummer und Name
            var firstRow = new Grid();
            firstRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            firstRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var customerNumberText = new TextBlock
            {
                Text = customer.CustomerNumber,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var nameText = new TextBlock
            {
                Text = customer.Name,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(customerNumberText, 0);
            Grid.SetColumn(nameText, 1);
            firstRow.Children.Add(customerNumberText);
            firstRow.Children.Add(nameText);

            // Zweite Zeile: Stadt und Land
            var secondRow = new Grid();
            secondRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            secondRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var locationText = new TextBlock
            {
                Text = $"{customer.City}",
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 190)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var countryText = new TextBlock
            {
                Text = customer.Country.ToString(),
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(locationText, 0);
            Grid.SetColumn(countryText, 1);
            secondRow.Children.Add(locationText);
            secondRow.Children.Add(countryText);

            // Dritte Zeile: E-Mail
            var emailText = new TextBlock
            {
                Text = customer.Email,
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 170)),
                FontSize = 11,
                Margin = new Thickness(0, 3, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            customerPanel.Children.Add(firstRow);
            customerPanel.Children.Add(secondRow);
            customerPanel.Children.Add(emailText);
            
            customerBorder.Child = customerPanel;

            // Hover-Effekt
            customerBorder.PointerEntered += OnPointerEntered;
            customerBorder.PointerExited += OnPointerExited;
            customerBorder.DetachedFromLogicalTree += OnDetachedFromLogicalTree;

            return customerBorder;
        });

        _customerListBox.ItemTemplate = itemTemplate;

        // Event Handler für Selektion
        _customerListBox.SelectionChanged += CustomerListBox_SelectionChanged;

        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        Grid.SetRow(headerPanel, 0);
        Grid.SetRow(scrollViewer, 1);
        contentGrid.Children.Add(headerPanel);
        contentGrid.Children.Add(scrollViewer);
        panel.Child = contentGrid;

        return panel;
    }

    private StackPanel CreateEditPanel()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(10, 0, 0, 0)
        };

        var editHeader = new TextBlock
        {
            Text = "Kunde bearbeiten",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var formBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(50, 50, 60)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20)
        };

        // ScrollViewer für das Formular hinzufügen
        var formScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            MaxHeight = 600
        };

        var formGrid = new Grid
        {
            ColumnDefinitions = 
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };
        
        for (var i = 0; i < 12; i++)
        {
            formGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        // Form Fields erstellen
        CreateFormField(formGrid, 0, "Kundennummer:", out _customerNumberBox);
        CreateFormField(formGrid, 1, "Name:", out _nameBox);
        CreateFormField(formGrid, 2, "Adresse:", out _addressBox);
        CreateFormField(formGrid, 3, "PLZ:", out _postalCodeBox);
        CreateFormField(formGrid, 4, "Stadt:", out _cityBox);
        
        // Land Dropdown
        CreateLabel(formGrid, 5, "Land:");
        _countryCombo = CreateComboBox();
        _countryCombo.ItemsSource = EnumCountries;

        _countryCombo.SelectionChanged += CountryCombo_SelectionChanged;
        Grid.SetRow(_countryCombo, 5);
        Grid.SetColumn(_countryCombo, 1);
        formGrid.Children.Add(_countryCombo);

        // UID mit Country Code
        CreateLabel(formGrid, 6, "UID:");
        var uidPanel = new StackPanel { Orientation = Orientation.Horizontal };
        _uidCountryCombo = CreateComboBox();
        _uidCountryCombo.ItemsSource = EnumCountryCodes;
        _uidCountryCombo.Width = 80;
        _uidCountryCombo.Margin = new Thickness(0, 0, 10, 0);
        
        _uidNumberBox = CreateTextBox();
        uidPanel.Children.Add(_uidCountryCombo);
        uidPanel.Children.Add(_uidNumberBox);
        Grid.SetRow(uidPanel, 6);
        Grid.SetColumn(uidPanel, 1);
        formGrid.Children.Add(uidPanel);

        CreateFormField(formGrid, 7, "Zahlungsziel (Tage):", out _paymentDueBox);
        CreateFormField(formGrid, 8, "E-Mail:", out _emailBox);

        // Net Calculation Type
        CreateLabel(formGrid, 9, "EU-Mitglied:");
        _netCalculationCombo = CreateComboBox();
        _netCalculationCombo.ItemsSource = EnumNetCalculationTypes;
        Grid.SetRow(_netCalculationCombo, 9);
        Grid.SetColumn(_netCalculationCombo, 1);
        formGrid.Children.Add(_netCalculationCombo);

        formScrollViewer.Content = formGrid;
        formBorder.Child = formScrollViewer;
        panel.Children.Add(editHeader);
        panel.Children.Add(formBorder);

        return panel;
    }
    
    private StackPanel CreateFooterPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        _newButton = CreateActionButton("Neuer Kunde", Color.FromRgb(46, 125, 50));
        _newButton.Click += NewButton_Click;

        _saveButton = CreateActionButton("Speichern", Color.FromRgb(25, 118, 210));
        _saveButton.Click += SaveButton_Click;

        _deleteButton = CreateActionButton("Löschen", Color.FromRgb(211, 47, 47));
        _deleteButton.Click += DeleteButton_Click;

        _cancelButton = CreateActionButton("Abbrechen", Color.FromRgb(97, 97, 97));
        _cancelButton.Click += CancelButton_Click;

        panel.Children.Add(_newButton);
        panel.Children.Add(_saveButton);
        panel.Children.Add(_deleteButton);
        panel.Children.Add(_cancelButton);

        return panel;
    }

    private Button CreateActionButton(string text, Color backgroundColor)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(20, 12),
            Margin = new Thickness(10, 0, 0, 0),
            Background = new SolidColorBrush(backgroundColor),
            Tag = backgroundColor,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            FontWeight = FontWeight.Medium
        };

        button.PointerEntered += OnPointerEntered;
        button.PointerExited += OnPointerExited;

        button.DetachedFromLogicalTree += OnDetachedFromLogicalTree;
        return button;
    }

    private static void PointerEffect(Button btn, bool entered)
    {
        if (!btn.IsEnabled || btn.Tag is not Color backgroundColor)
            return;

        var brighterColor = Color.FromRgb(
            (byte)Math.Min(255, backgroundColor.R + 20),
            (byte)Math.Min(255, backgroundColor.G + 20),
            (byte)Math.Min(255, backgroundColor.B + 20));

        btn.Background = new SolidColorBrush(entered ? brighterColor : backgroundColor);
    }

    private static void PointerEffect(Border border, bool entered)
    {
        border.Background = entered ? new SolidColorBrush(Color.FromRgb(50, 50, 60)) : new SolidColorBrush(Color.FromRgb(40, 40, 50));
    }
   
    private void CreateFormField(Grid grid, int row, string labelText, out TextBox textBox)
    {
        CreateLabel(grid, row, labelText);
        textBox = CreateTextBox();
        Grid.SetRow(textBox, row);
        Grid.SetColumn(textBox, 1);
        grid.Children.Add(textBox);
    }

    private void CreateLabel(Grid grid, int row, string text)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 15, 0),
            FontWeight = FontWeight.Medium
        };
        Grid.SetRow(label, row);
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);
    }

    private TextBox CreateTextBox()
    {
        return new TextBox
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 55)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 5),
            FontSize = 14
        };
    }

    private ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 55)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 5),
            FontSize = 14
        };
    }

    private void LoadCustomerData()
    {
        if (!File.Exists(FILE_PATH))
        {
            Logger.Log("First time loading customer data, file not found.");
            return;
        }
        
        var lines = File.ReadAllLines(FILE_PATH);
        for (var i = 1; i < lines.Length; i++)
        {
            var customer = Customer.ReadCsvLine(lines[i]);
            if (customer != null)
            {
                _customers.Add(customer);
            }
            else
            {
                Logger.Warn($"Invalid customer data at line {i + 1} in {FILE_PATH}");
            }
        }
        
        _countOfCustomersOnLoad = _customers.Count;
        
        Logger.Log(
            _customers.Count == 0
                ? "No customers found in the database."
                : $"{_customers.Count} customers loaded from {FILE_PATH}");
    }

    private void EnableForm(bool enable = true)
    {
        _customerNumberBox.IsEnabled = enable;
        _nameBox.IsEnabled = enable;
        _addressBox.IsEnabled = enable;
        _postalCodeBox.IsEnabled = enable;
        _cityBox.IsEnabled = enable;
        _uidNumberBox.IsEnabled = enable;
        _emailBox.IsEnabled = enable;
        _paymentDueBox.IsEnabled = enable;
        _countryCombo.IsEnabled = enable;
        _uidCountryCombo.IsEnabled = enable;
        _netCalculationCombo.IsEnabled = enable;
        
        _saveButton.IsEnabled = enable;
        _deleteButton.IsEnabled = enable && _selectedCustomer != null;
        _cancelButton.IsEnabled = enable;
        
        _isEditing = enable;    
    }
    
    private void ClearForm()
    {
        _customerNumberBox.Text = string.Empty;
        _nameBox.Text = string.Empty;
        _addressBox.Text = string.Empty;
        _postalCodeBox.Text = string.Empty;
        _cityBox.Text = string.Empty;
        _uidNumberBox.Text = string.Empty;
        _emailBox.Text = string.Empty;
        _paymentDueBox.Text = string.Empty;
        _countryCombo.SelectedIndex = -1;
        _uidCountryCombo.SelectedIndex = -1;
        _netCalculationCombo.SelectedIndex = -1;
        
        if (_customerListBox.SelectedItem == null) return;
        _customerListBox.SelectedItem = null;
        _selectedCustomer = null;
    }

    private bool RequiredFieldsAreFilled()
    {
        return !string.IsNullOrWhiteSpace(_customerNumberBox.Text) &&
               !string.IsNullOrWhiteSpace(_nameBox.Text) &&
               !string.IsNullOrWhiteSpace(_addressBox.Text) &&
               !string.IsNullOrWhiteSpace(_postalCodeBox.Text) &&
               !string.IsNullOrWhiteSpace(_cityBox.Text) &&
               !string.IsNullOrWhiteSpace(_uidNumberBox.Text) &&
               !string.IsNullOrWhiteSpace(_emailBox.Text) &&
               _countryCombo.SelectedIndex >= 0 &&
               _uidCountryCombo.SelectedIndex >= 0 &&
               _netCalculationCombo.SelectedIndex >= 0 &&
               int.TryParse(_paymentDueBox.Text?.Trim(), out _);
    }

    private void SaveCustomerData()
    {
        using (var writer = new StreamWriter(FILE_PATH, false))
        {
            writer.WriteLine("Kundennummer;Name;Adresse;PLZ;Stadt;Land;UID;Zahlungsziel;E-Mail;EU-Mitglied");
            foreach (var customer in _customers)
            {
                writer.WriteLine(customer.ToCsvLine());
            }
        }
        
        Logger.Log($"Customer data saved to {FILE_PATH}");
    }
    
    private async void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if(_countOfCustomersOnLoad != _customers.Count)
            {
                var result = await MessageBox.ShowYesNo("Veränderungen an Kundendaten","Es wurden Änderungen an den Kundendaten vorgenommen. \nMöchten Sie diese speichern?");
                if(!result) NavigationRequested?.Invoke(this, ViewType.Main);
                else SaveCustomerData();
            }
        
            NavigationRequested?.Invoke(this, ViewType.Main);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", "Ein Fehler ist aufgetreten beim Navigieren zurück zur Hauptansicht.");
            Logger.Error("Error navigating back: " + ex.Message);
        }
    }

    private void CustomerListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_customerListBox.SelectedItem is not Customer selectedCustomer)
        {
            ClearForm();
            EnableForm(false);
            return;
        }

        _selectedCustomer = selectedCustomer;
        EnableForm();

        _customerNumberBox.Text = selectedCustomer.CustomerNumber;
        _nameBox.Text = selectedCustomer.Name;
        _addressBox.Text = selectedCustomer.Address;
        _postalCodeBox.Text = selectedCustomer.PostalCode;
        _cityBox.Text = selectedCustomer.City;
        _countryCombo.SelectedItem = selectedCustomer.Country;
        
        if (Enum.TryParse<CountryCode>(selectedCustomer.Uid[..2], out var countryCode))
        {
            _uidCountryCombo.SelectedItem = countryCode;
            _uidNumberBox.Text = selectedCustomer.Uid[2..];
        }
        
        _paymentDueBox.Text = (selectedCustomer.PaymentDueDate - DateTime.Today).Days.ToString();
        _emailBox.Text = selectedCustomer.Email;
        _netCalculationCombo.SelectedItem = selectedCustomer.NetCalculationType;
    }

    private void NewButton_Click(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        EnableForm();
        _newButton.IsEnabled = false;

        if (_customers.Count == 0)
        {
            _customerNumberBox.Text = "K01";
        }
        
        var getHighestNumber = _customers.Select(c => c.CustomerNumber)
                .Select(num => int.TryParse(num.AsSpan(1), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
        
        _customerNumberBox.Text = $"K{getHighestNumber + 1:D2}";
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!_isEditing || !RequiredFieldsAreFilled())
            {
                Logger.Warn("Cannot save - not editing or required fields missing.");
                return;
            }

            if (CountryCodes.GetCountryCode((Country)_countryCombo.SelectedValue!) !=
                (CountryCode)_uidCountryCombo.SelectedValue!)
            {
                Logger.Warn("Country code does not match selected country.");
                var result = await MessageBox.ShowYesNo("Warnung", "Das ausgewählte Land stimmt nicht mit dem Ländercode der UID überein. \nBehalten?");
                if (!result) return;
            }
            
            var customer = new Customer(
                _customerNumberBox.Text!.Trim(),
                _nameBox.Text!.Trim(),
                _addressBox.Text!.Trim(),
                _postalCodeBox.Text!.Trim(),
                _cityBox.Text!.Trim(),
                (Country)_countryCombo.SelectedItem!,
                $"{_uidCountryCombo.SelectedItem}{_uidNumberBox.Text!.Trim()}",
                int.Parse(_paymentDueBox.Text!.Trim()),
                _emailBox.Text!.Trim(),
                (NetCalculationType)_netCalculationCombo.SelectedItem!);

            if (_selectedCustomer != null)
            {
                var index = _customers.IndexOf(_selectedCustomer);
                _customers[index] = customer;
            }
            else
            {
                _customers.Add(customer);
            }
    
            ClearForm();
            EnableForm(false);
            _newButton.IsEnabled = true;
        
            Logger.Log("Customer saved successfully.");
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", ex.Message);
            Logger.Error("Error saving customer: " + ex.Message);
        }
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_selectedCustomer == null)
            {
                Logger.Warn("No customer selected for deletion.");
                return;
            }

            var customerToDelete = _selectedCustomer;

            var result = await MessageBox.ShowYesNo("Kunden löschen",
                $"Möchten Sie den Kunden '{customerToDelete.Name}' wirklich löschen?");

            if (!result) return;

            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (customerToDelete == null)
            {
                Logger.Warn("Selected customer is null, cannot delete.");
                await MessageBox.ShowError("Fehler", "Der ausgewählte Kunde ist nicht mehr verfügbar.");
                return;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (_customers == null)
            {
                Logger.Warn("Customer list is null, cannot delete customer.");
                await MessageBox.ShowError("Fehler", "Die Kundenliste ist leer oder nicht initialisiert.");
                return;
            }
            
            //this shit gives null reference which is not possible
            _customers.Remove(customerToDelete);
            ClearForm();
            EnableForm(false);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", "Ein Fehler ist aufgetreten beim Löschen des Kunden.");
            Logger.Error("Error deleting customer: " + ex.Message);
        }
    }
    
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        EnableForm(false);
        _newButton.IsEnabled = true;
    }

    private void CountryCombo_SelectionChanged(object? sender, RoutedEventArgs e)
    {
        if (_countryCombo.SelectedItem is not Country country|| !_isEditing) return;
        _uidCountryCombo.SelectedIndex = (int)CountryCodes.GetCountryCode(country);
    }
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.S || e.KeyModifiers != KeyModifiers.Control) return;
        e.Handled = true;
        _countOfCustomersOnLoad = _customers.Count;
        SaveCustomerData();
    }

    private void OnPointerEntered(object? sender, RoutedEventArgs e)
    {
        switch (sender)
        {
            case Button btn:
                PointerEffect(btn, true);
                break;
            case Border border:
                PointerEffect(border, true);
                break;
            default:
                return;
        }
    }
    private void OnPointerExited(object? sender, RoutedEventArgs e)
    {
        switch (sender)
        {
            case Button btn:
                PointerEffect(btn, false);
                break;
            case Border border:
                PointerEffect(border, false);
                break;
            default:
                return;
        }
    }
    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        switch (sender)
        {
            case Button btn:
                btn.PointerEntered -= OnPointerEntered;
                btn.PointerExited -= OnPointerExited;
                btn.DetachedFromLogicalTree -= OnDetachedFromLogicalTree;
                break;
            case Border border:
                border.PointerEntered -= OnPointerEntered;
                border.PointerExited -= OnPointerExited;
                border.DetachedFromLogicalTree -= OnDetachedFromLogicalTree;
                break;
        }
    }
    public void Dispose()
    {
        NavigationRequested = null;

        _customerListBox.SelectionChanged -= CustomerListBox_SelectionChanged;
        _countryCombo.SelectionChanged -= CountryCombo_SelectionChanged;
        
        _cancelButton.Click -= CancelButton_Click;
        _deleteButton.Click -= DeleteButton_Click;
        _saveButton.Click -= SaveButton_Click;
        _newButton.Click -= NewButton_Click;
        _backButton.Click -= BackButton_Click;

        _customerListBox.ItemsSource = null;
        
        _customers.Clear();
        _customers = null!;

        _customerListBox = null!;
        _editPanel = null!;
        _customerNumberBox = null!;
        _nameBox = null!;
        _addressBox = null!;
        _postalCodeBox = null!;
        _cityBox = null!;
        _uidNumberBox = null!;
        _emailBox = null!;
        _paymentDueBox = null!;
        _countryCombo = null!;
        _uidCountryCombo = null!;
        _netCalculationCombo = null!;
        _saveButton = null!;
        _deleteButton = null!;
        _newButton = null!;
        _cancelButton = null!;
        
        KeyDown -= OnKeyDown;
        
        (Content as Grid)?.Children.Clear();
        Content = null;
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Logger.Log("CustomerView disposed.");
    }

}