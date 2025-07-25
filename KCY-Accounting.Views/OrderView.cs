using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;
using KCY_Accounting.Logic;

namespace KCY_Accounting.Views;

public class OrderView : UserControl, IView
{
    private const string FILE_PATH = "resources/appdata/orders.kdb";
    private const string CUSTOMERS_FILE_PATH = "resources/appdata/customers.kdb";

    public string Title => "KCY-Accounting - Auftragsansicht";
    public WindowIcon Icon => new("resources/pictures/order-management.ico");

    public event EventHandler<ViewType>? NavigationRequested;

    private static readonly FreightType[] EnumFreightTypes = Enum.GetValues<FreightType>();
    private static readonly NetCalculationType[] EnumNetCalculationTypes = Enum.GetValues<NetCalculationType>();

    private readonly ObservableCollection<Order> _orders = [];
    private readonly ObservableCollection<Customer> _customers = [];
    private int _countOfOrdersOnLoad;

    private ListBox _orderListBox;
    private StackPanel _editPanel;

    private TextBox _invoiceNumberBox,
        _customerNumberBox,
        _invoiceReferenceBox,
        _routeFromBox,
        _routeToBox,
        _driverNameBox,
        _driverLastNameBox,
        _driverLicensePlateBox,
        _driverPhoneBox,
        _netAmountBox,
        _descriptionBox;

    private DatePicker _orderDatePicker, _serviceeDatePicker, _driverBirthdayPicker;
    private ComboBox _customerCombo, _freightTypeCombo, _taxStatusCombo;
    private CheckBox _podsCheckBox;
    private TextBlock _taxAmountLabel, _grossAmountLabel;
    private Button _saveButton, _deleteButton, _newButton, _cancelButton;
    private bool _isEditing;
    private Order? _selectedOrder;

    public void Init()
    {
        LoadCustomerData();
        LoadOrderData();

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

        var backButton = new Button
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
        backButton.Click += BackButton_Click;

        KeyDown += OnKeyDown;

        var titleText = new TextBlock
        {
            Text = "Auftragsverwaltung",
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

        Grid.SetColumn(backButton, 0);
        Grid.SetColumn(titleText, 1);
        Grid.SetColumn(saveHint, 2);
        headerPanel.Children.Add(backButton);
        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(saveHint);

        // Hauptcontent Grid
        var contentGrid = new Grid();
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(2, GridUnitType.Star));
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

        // Linke Seite - Auftragsliste
        var leftPanel = CreateOrderListPanel();

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

        Logger.Log($"UI loaded with {_orders.Count} orders and {_customers.Count} customers.");
    }
    
    private Border CreateOrderListPanel()
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

        // Header mit Auftragsliste-Titel und Anzahl
        var headerPanel = new Grid();
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var listHeader = new TextBlock
        {
            Text = "Auftragsliste",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        var orderCountText = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Binding für die Auftragsanzahl
        orderCountText.Bind(TextBlock.TextProperty,
            new Binding("Count") { Source = _orders, StringFormat = "{0} Aufträge" });

        Grid.SetColumn(listHeader, 0);
        Grid.SetColumn(orderCountText, 1);
        headerPanel.Children.Add(listHeader);
        headerPanel.Children.Add(orderCountText);

        // ScrollViewer für die ListBox
        _orderListBox = new ListBox
        {
            Background = Brushes.Transparent,
            SelectionMode = SelectionMode.Single,
            ItemsSource = _orders
        };

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 15, 0, 0),
            Content = _orderListBox
        };

        var itemTemplate = new FuncDataTemplate<Order>((order, _) =>
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (order == null) return null;

            var orderBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10),
                Margin = new Thickness(0, 2)
            };

            var orderPanel = new StackPanel();

            var firstRow = new Grid();
            firstRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            firstRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            firstRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var invoiceNumberText = new TextBlock
            {
                Text = order.InvoiceNumber,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 200, 100)),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var customerText = new TextBlock
            {
                Text = order.Customer.Name,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(15, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var orderDateText = new TextBlock
            {
                Text = order.OrderDate.ToString("dd.MM.yy"),
                FontWeight = FontWeight.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(invoiceNumberText, 0);
            Grid.SetColumn(customerText, 1);
            Grid.SetColumn(orderDateText, 2);
            firstRow.Children.Add(invoiceNumberText);
            firstRow.Children.Add(customerText);
            firstRow.Children.Add(orderDateText);

            // Zweite Zeile: Route und Frachttyp
            var secondRow = new Grid();
            secondRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            secondRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var routeText = new TextBlock
            {
                Text = $"{order.Route.From} → {order.Route.To}",
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 190)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var freightTypeText = new TextBlock
            {
                Text = order.FreightType.ToString(),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 170, 100)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(routeText, 0);
            Grid.SetColumn(freightTypeText, 1);
            secondRow.Children.Add(routeText);
            secondRow.Children.Add(freightTypeText);

            var thirdRow = new Grid();
            thirdRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            thirdRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var driverText = new TextBlock
            {
                Text = $"{order.Driver.FirstName} {order.Driver.LastName}",
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 170)),
                FontSize = 11,
                Margin = new Thickness(0, 3, 0, 0)
            };

            var amountText = new TextBlock
            {
                Text = $"€ {order.GrossAmount:F2}",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 3, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(driverText, 0);
            Grid.SetColumn(amountText, 1);
            thirdRow.Children.Add(driverText);
            thirdRow.Children.Add(amountText);

            orderPanel.Children.Add(firstRow);
            orderPanel.Children.Add(secondRow);
            orderPanel.Children.Add(thirdRow);

            orderBorder.Child = orderPanel;

            // Hover-Effekt
            orderBorder.PointerEntered += OnPointerEntered;
            orderBorder.PointerExited += OnPointerEntered;
            orderBorder.DetachedFromLogicalTree += OnDetachedFromLogicalTree;
            
            return orderBorder;
        });

        _orderListBox.ItemTemplate = itemTemplate;

        _orderListBox.SelectionChanged += OrderListBox_SelectionChanged;

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
            Text = "Auftrag bearbeiten",
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

        for (var i = 0; i < 18; i++)
        {
            formGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        CreateSectionHeader(formGrid, 0, "Grunddaten");
        CreateFormField(formGrid, 1, "Rechnungsnummer:", out _invoiceNumberBox);

        CreateLabel(formGrid, 2, "Auftragsdatum:");
        _orderDatePicker = CreateDatePicker();
        Grid.SetRow(_orderDatePicker, 2);
        Grid.SetColumn(_orderDatePicker, 1);
        formGrid.Children.Add(_orderDatePicker);

        CreateLabel(formGrid, 3, "Kunde:");
        _customerCombo = CreateComboBox();
        _customerCombo.SelectionChanged += CustomerCombo_SelectionChanged;
        _customerCombo.ItemsSource = _customers;
        _customerCombo.DisplayMemberBinding = new Binding("Name");
        Grid.SetRow(_customerCombo, 3);
        Grid.SetColumn(_customerCombo, 1);
        formGrid.Children.Add(_customerCombo);
        
        CreateFormField(formGrid, 4, "Kundennummer:", out _customerNumberBox);
        CreateFormField(formGrid, 5, "Rechnungsreferenz:", out _invoiceReferenceBox);

        CreateSectionHeader(formGrid, 6, "Route");
        CreateFormField(formGrid, 7, "Von:", out _routeFromBox);
        CreateFormField(formGrid, 8, "Nach:", out _routeToBox);

        CreateLabel(formGrid, 9, "Leistungsdatum:");
        _serviceeDatePicker = CreateDatePicker();
        Grid.SetRow(_serviceeDatePicker, 9);
        Grid.SetColumn(_serviceeDatePicker, 1);  
        formGrid.Children.Add(_serviceeDatePicker);

        CreateSectionHeader(formGrid, 10, "Fahrer");
        CreateFormField(formGrid, 11, "Vorname:", out _driverNameBox);
        CreateFormField(formGrid, 12, "Nachname:", out _driverLastNameBox);
        CreateFormField(formGrid, 13, "Kennzeichen:", out _driverLicensePlateBox);

        CreateLabel(formGrid, 14, "Geburtsdatum:");
        _driverBirthdayPicker = CreateDatePicker();
        Grid.SetRow(_driverBirthdayPicker, 14);
        Grid.SetColumn(_driverBirthdayPicker, 1);
        formGrid.Children.Add(_driverBirthdayPicker);

        CreateFormField(formGrid, 15, "Telefon:", out _driverPhoneBox);

        CreateSectionHeader(formGrid, 16, "Fracht & Finanzen");

        CreateLabel(formGrid, 17, "Frachttyp:");
        _freightTypeCombo = CreateComboBox();
        _freightTypeCombo.ItemsSource = EnumFreightTypes;
        Grid.SetRow(_freightTypeCombo, 17);
        Grid.SetColumn(_freightTypeCombo, 1);
        formGrid.Children.Add(_freightTypeCombo);

        CreateLabel(formGrid, 18, "PODS:");
        _podsCheckBox = new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5)
        };
        Grid.SetRow(_podsCheckBox, 18);
        Grid.SetColumn(_podsCheckBox, 1);
        formGrid.Children.Add(_podsCheckBox);

        for (var i = 18; i < 24; i++)
        {
            formGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        CreateFormField(formGrid, 19, "Nettobetrag (€):", out _netAmountBox);
        _netAmountBox.TextChanged += NetAmountBox_TextChanged;

        CreateLabel(formGrid, 20, "Steuerstatus:");
        _taxStatusCombo = CreateComboBox();
        _taxStatusCombo.ItemsSource = EnumNetCalculationTypes;
        Grid.SetRow(_taxStatusCombo, 20);
        Grid.SetColumn(_taxStatusCombo, 1);
        formGrid.Children.Add(_taxStatusCombo);

        _taxStatusCombo.SelectionChanged += OnStatusChanged; 
    
        _orderDatePicker.TabIndex = -1;
        _serviceeDatePicker.TabIndex = -1;
        _driverBirthdayPicker.TabIndex = -1;
        _podsCheckBox.TabIndex = -1;
    
        _taxAmountLabel = new TextBlock
        {
            Text = "€ 0,00",
            Foreground = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
            FontWeight = FontWeight.Medium,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5)
        };
    
        
        _grossAmountLabel = new TextBlock
        {
            Text = "€ 0,00",
            Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5)
        };
        
        CreateLabel(formGrid, 21, "Steuerbetrag (€):");
        Grid.SetRow(_taxAmountLabel, 21);
        Grid.SetColumn(_taxAmountLabel, 1);
        formGrid.Children.Add(_taxAmountLabel);
        
        CreateLabel(formGrid, 22, "Bruttobetrag (€):");
        Grid.SetRow(_grossAmountLabel, 22);
        Grid.SetColumn(_grossAmountLabel, 1);
        formGrid.Children.Add(_grossAmountLabel);
        
        CreateLabel(formGrid, 23, "Beschreibung:");
        _descriptionBox = CreateTextBox();
        _descriptionBox.AcceptsReturn = true;
        _descriptionBox.TextWrapping = TextWrapping.Wrap;
        _descriptionBox.Height = 80;
        Grid.SetRow(_descriptionBox, 23);
        Grid.SetColumn(_descriptionBox, 1);
        formGrid.Children.Add(_descriptionBox);

        formScrollViewer.Content = formGrid;
        formBorder.Child = formScrollViewer;
        panel.Children.Add(editHeader);
        panel.Children.Add(formBorder);

        return panel;
    }

    private void OnStatusChanged(object? sender, SelectionChangedEventArgs e)
    {
        NetAmountBox_TextChanged(null, null!);
    }
    private StackPanel CreateFooterPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        _newButton = CreateActionButton("Neuer Auftrag", Color.FromRgb(46, 125, 50));
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
            Foreground = Brushes.White,
            Tag = backgroundColor,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            FontWeight = FontWeight.Medium
        };

        button.PointerEntered += OnPointerEntered;
        button.PointerExited += OnPointerExited;
        button.DetachedFromLogicalTree += OnDetachedFromLogicalTree;
        
        return button;
    }
    
    private static void PointerEffect(Border border, bool entered)
    {
        border.Background = entered ? new SolidColorBrush(Color.FromRgb(50, 50, 60)) :  new SolidColorBrush(Color.FromRgb(40, 40, 50));
    }
    private static void PointerEffect(Button btn, bool entered)
    {
        if (!btn.IsEnabled || btn.Tag is not Color backgroundColor) return;

        var brighterColor = Color.FromRgb(
            (byte)Math.Min(255, backgroundColor.R + 20),
            (byte)Math.Min(255, backgroundColor.G + 20),
            (byte)Math.Min(255, backgroundColor.B + 20)
        );
        
        btn.Background = new SolidColorBrush(entered ? brighterColor : backgroundColor);
    }
    private void CreateSectionHeader(Grid grid, int row, string text)
    {
        var header = new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            Margin = new Thickness(0, 15, 0, 10),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(header, row);
        Grid.SetColumn(header, 0);
        Grid.SetColumnSpan(header, 2);
        grid.Children.Add(header);
    }

    private void CreateFormField(Grid grid, int row, string labelText, out TextBox textBox)
    {
        CreateLabel(grid, row, labelText);
        textBox = CreateTextBox();
        Grid.SetRow(textBox, row);
        Grid.SetColumn(textBox, 1);
        grid.Children.Add(textBox);
    }

    private static void CreateLabel(Grid grid, int row, string text)
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

    private static TextBox CreateTextBox()
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

    private static ComboBox CreateComboBox()
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

    private static DatePicker CreateDatePicker()
    {
        return new DatePicker
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 55)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 5),
            FontSize = 14
        };
    }

    private void LoadCustomerData()
    {
        if (!File.Exists(CUSTOMERS_FILE_PATH))
        {
            Logger.Log("First time loading customer data, file not found.");
            return;
        }
        
        var lines = File.ReadAllLines(CUSTOMERS_FILE_PATH);
        for (var i = 1; i < lines.Length; i++)
        {
            var customer = Customer.ReadCsvLine(lines[i]);
            if (customer != null)
            {
                _customers.Add(customer);
            }
            else
            {
                Logger.Warn($"Invalid customer data at line {i + 1} in {CUSTOMERS_FILE_PATH}");
            }
        }
        
        Logger.Log(
            _customers.Count == 0
                ? "No customers found in the database."
                : $"{_customers.Count} customers loaded from {CUSTOMERS_FILE_PATH}");
    }

    private void LoadOrderData()
    {
        if (!File.Exists(FILE_PATH))
        {
            Logger.Log("First time loading order data, file not found.");
            return;
        }

        var lines = File.ReadAllLines(FILE_PATH);
        for (var i = 1; i < lines.Length; i++)
        {
            var order = Order.ReadCsvLine(lines[i], _customers.ToArray());
            if (order != null)
            {
                _orders.Add(order);
            }
            else
            {
                Logger.Warn($"Invalid order data at line {i + 1} in {FILE_PATH}");
            }
        }

        _countOfOrdersOnLoad = _orders.Count;

        Logger.Log(
            _orders.Count == 0
                ? "No orders found in the database."
                : $"{_orders.Count} orders loaded from {FILE_PATH}");
    }

    private void SaveOrderData()
    {
        using (var writer = new StreamWriter(FILE_PATH,false))
        {
            writer.WriteLine("Rechnungsnummer;Auftragsdatum;Kundennummer;Kunden(Name);Rechnungsnummer;Von Bis;Leistungsdatum;Fahrername Nachname Kennzeichen Geburtstag Tel;Frachttyp;PODS;NettoBetrag;Steuerstatus;Notiz");
            foreach (var order in _orders)
            {
                writer.WriteLine(order.ToCsvLine());
            }
        }
        
        Logger.Log($"Customer data saved to {FILE_PATH}");
    }

    private bool RequiredFieldsAreFilled()
    {
        return
            _invoiceNumberBox.Text != null && !string.IsNullOrWhiteSpace(_invoiceNumberBox.Text) &&
            _invoiceReferenceBox.Text != null && !string.IsNullOrWhiteSpace(_invoiceReferenceBox.Text) &&
            _routeFromBox.Text != null && !string.IsNullOrWhiteSpace(_routeFromBox.Text) &&
            _routeToBox.Text != null && !string.IsNullOrWhiteSpace(_routeToBox.Text) &&
            _driverNameBox.Text != null && !string.IsNullOrWhiteSpace(_driverNameBox.Text) &&
            _driverLastNameBox.Text != null && !string.IsNullOrWhiteSpace(_driverLastNameBox.Text) &&
            _driverLicensePlateBox.Text != null && !string.IsNullOrWhiteSpace(_driverLicensePlateBox.Text) &&
            _driverPhoneBox.Text != null && !string.IsNullOrWhiteSpace(_driverPhoneBox.Text) && _freightTypeCombo.SelectedIndex >= 0 && _taxStatusCombo.SelectedIndex >= 0 &&
            _netAmountBox.Text != null && !string.IsNullOrWhiteSpace(_netAmountBox.Text) &&
            _customerNumberBox.Text != null && !string.IsNullOrWhiteSpace(_customerNumberBox.Text);
    }
    
    private void EnableForm(bool enable = true)
    {
        _invoiceNumberBox.IsEnabled = enable;
        _customerNumberBox.IsEnabled = enable;
        _orderDatePicker.IsEnabled = enable;
        _customerCombo.IsEnabled = enable;
        _invoiceReferenceBox.IsEnabled = enable;
        _routeFromBox.IsEnabled = enable;
        _routeToBox.IsEnabled = enable;
        _serviceeDatePicker.IsEnabled = enable;
        _driverNameBox.IsEnabled = enable;
        _driverLastNameBox.IsEnabled = enable;
        _driverLicensePlateBox.IsEnabled = enable;
        _driverBirthdayPicker.IsEnabled = enable;
        _driverPhoneBox.IsEnabled = enable;
        _freightTypeCombo.IsEnabled = enable;
        _taxStatusCombo.IsEnabled = enable;
        _podsCheckBox.IsEnabled = enable;
        _taxAmountLabel.IsEnabled = enable;
        _grossAmountLabel.IsEnabled = enable;

        _saveButton.IsEnabled = enable;
        _deleteButton.IsEnabled = enable && _selectedOrder != null;
        _cancelButton.IsEnabled = enable;

        _isEditing = enable;
    }

    private void ClearForm()
    {
        _invoiceNumberBox.Text = string.Empty;
        _customerNumberBox.Text = string.Empty;
        _orderDatePicker.SelectedDate = null;
        _customerCombo.SelectedIndex = -1;
        _invoiceReferenceBox.Text = string.Empty;
        _routeFromBox.Text = string.Empty;
        _routeToBox.Text = string.Empty;
        _serviceeDatePicker.SelectedDate = null;
        _driverNameBox.Text = string.Empty;
        _driverLastNameBox.Text = string.Empty;
        _driverLicensePlateBox.Text = string.Empty;
        _driverBirthdayPicker.SelectedDate = null;
        _driverPhoneBox.Text = string.Empty;
        _freightTypeCombo.SelectedIndex = -1;
        _taxStatusCombo.SelectedIndex = -1;
        _podsCheckBox.IsChecked = false;
        _netAmountBox.Text = string.Empty;
        _taxAmountLabel.Text = "€ 0,00";
        _grossAmountLabel.Text = "€ 0,00";
        _descriptionBox.Text = string.Empty;

        if (_orderListBox.SelectedItem == null) return;
        _orderListBox.SelectedItem = null;
        _selectedOrder = null;
    }
    
    private void NewButton_Click(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        EnableForm();
        _newButton.IsEnabled = false;

        if (_orders.Count == 0)
        {
            _invoiceNumberBox.Text = "2025-01";
        }
        
        var getHighestNumber = _orders
            .Select(o => o.InvoiceNumber)
            .Select(s => {
                var parts = s.Split('-');
                return parts.Length > 1 && int.TryParse(parts[1], out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();
        
        _invoiceNumberBox.Text = $"{DateTime.Today.Year}-{getHighestNumber + 1:D2}";
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

            if (_customerCombo.SelectedItem == null)
            {
                await MessageBox.ShowError("Fehler", "Bitte wählen Sie einen Kunden aus.");
                return;
            }

            var customer = (Customer)_customerCombo.SelectedItem;

            if (!_serviceeDatePicker.SelectedDate.HasValue || !_orderDatePicker.SelectedDate.HasValue ||
                !_driverBirthdayPicker.SelectedDate.HasValue)
            {
                await MessageBox.ShowError("Fehler", "Bitte füllen Sie alle Datumsfelder aus.");
                return;
            }

            if (_serviceeDatePicker.SelectedDate != customer.PaymentDueDate)
            {
                Logger.Warn("Customer PaymentDue does not fit to selected service Date");
                var result = await MessageBox.ShowYesNo("Warnung",
                    $"Das ausgewählte Datum stimmt nicht überein mit der Zahlungsfrist ({DateTime.Today - customer.PaymentDueDate}) überein. \nBehalten?");
                if (!result) return;
            }

            var order = new Order(_invoiceNumberBox.Text!,
                _orderDatePicker.SelectedDate.Value.Date,
                customer.CustomerNumber,
                customer,
                int.Parse(_invoiceReferenceBox.Text!),
                new Route(_routeFromBox.Text!, _routeToBox.Text!),
                _serviceeDatePicker.SelectedDate.Value.Date,
                new Driver(_driverNameBox.Text!, _driverLastNameBox.Text!, _driverLicensePlateBox.Text!,
                    _driverBirthdayPicker.SelectedDate.Value.Date, _driverPhoneBox.Text!),
                (FreightType)_freightTypeCombo.SelectedItem!,
                _podsCheckBox.IsChecked ?? false,
                float.Parse(_netAmountBox.Text!),
                (NetCalculationType)_taxStatusCombo.SelectedItem!,
                _descriptionBox.Text!);

            if (_selectedOrder != null)
            {
                var index = _orders.IndexOf(_selectedOrder);
                _orders[index] = order;
            }
            else
            {
                _orders.Add(order);
            }

            ClearForm();
            EnableForm(false);
            _newButton.IsEnabled = true;

            Logger.Log("Order saved successfully.");
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", ex.Message);
            Logger.Error("Error saving order: " + ex.Message);
        }
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_selectedOrder == null)
            {
                Logger.Warn("No order selected for deletion.");
                return;
            }

            var orderToDelete = _selectedOrder;

            var result = await MessageBox.ShowYesNo("Auftrag löschen",
                $"Möchten Sie den Auftrag wirklich löschen?");

            if (!result) return;

            if (orderToDelete == null)
            {
                Logger.Warn("Selected customer is null, cannot delete.");
                await MessageBox.ShowError("Fehler", "Der ausgewählte Kunde ist nicht mehr verfügbar.");
                return;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (_orders == null)
            {
                Logger.Warn("Order list is null, cannot delete order.");
                await MessageBox.ShowError("Fehler", "Die Auftragsliste ist leer oder nicht initialisiert.");
                return;
            }
            
            _orders.Remove(orderToDelete);
            _selectedOrder = null;
            ClearForm();
            EnableForm(false);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", "Ein Fehler ist aufgetreten beim Löschen der Order.");
            Logger.Error("Error deleting order: " + ex.Message);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        ClearForm();
        EnableForm(false);
        _newButton.IsEnabled = true;
    }
    private void NetAmountBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!float.TryParse(_netAmountBox.Text, out var net))
        {
            _taxAmountLabel.Text = "€ 0,00";
            _grossAmountLabel.Text = "€ 0,00";
            return;
        }

        if (_taxStatusCombo.SelectedItem is NetCalculationType.Yes)
        {
            var taxes = net * 0.2f;
            _taxAmountLabel.Text = $"€ {taxes:F2}";
            _grossAmountLabel.Text = $"€ {(net + taxes):F2}";
        }
        else
        {
            _taxAmountLabel.Text = "€ 0,00";
            _grossAmountLabel.Text = $"€ {net:F2}";
        }
    }
    
    private void OrderListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_orderListBox.SelectedItem is not Order selectedOrder)
        {
            ClearForm();
            EnableForm(false);
            return;
        }
        
        _selectedOrder = selectedOrder;
        
        _invoiceNumberBox.Text = selectedOrder.InvoiceNumber;
        _invoiceReferenceBox.Text = selectedOrder.InvoiceReference.ToString();
        _orderDatePicker.SelectedDate = selectedOrder.OrderDate;
        _customerCombo.SelectedItem = selectedOrder.Customer;
        _customerNumberBox.Text = selectedOrder.Customer.CustomerNumber;
        _routeFromBox.Text = selectedOrder.Route.From;
        _routeToBox.Text = selectedOrder.Route.To;
        _driverNameBox.Text = selectedOrder.Driver.FirstName;
        _driverLastNameBox.Text = selectedOrder.Driver.LastName;
        _driverLicensePlateBox.Text = selectedOrder.Driver.LicenseNumber;
        _driverBirthdayPicker.SelectedDate = selectedOrder.Driver.DateOfBirth;
        _driverPhoneBox.Text = selectedOrder.Driver.PhoneNumber;
        _netAmountBox.Text = selectedOrder.NetAmount.ToString(CultureInfo.InvariantCulture);
        _grossAmountLabel.Text = $"€ {selectedOrder.GrossAmount:F2}";
        _taxAmountLabel.Text = $"€ {selectedOrder.TaxAmount:F2}";
        _taxStatusCombo.SelectedItem = selectedOrder.TaxStatus;
        
        _freightTypeCombo.SelectedItem = selectedOrder.FreightType;
        _podsCheckBox.IsChecked = selectedOrder.Pods;
        _serviceeDatePicker.SelectedDate = selectedOrder.DateOfService;
        
        _descriptionBox.Text = selectedOrder.Description;
        
        EnableForm();
        _newButton.IsEnabled = false;
    }
    
    private async void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if(_countOfOrdersOnLoad != _orders.Count)
            {
                var result = await MessageBox.ShowYesNo("Veränderungen an Auftragsdaten","Es wurden Änderungen an den Auftragsdaten vorgenommen. \nMöchten Sie diese speichern?");
                if(!result) NavigationRequested?.Invoke(this, ViewType.Main);
                else SaveOrderData();
            }
        
            NavigationRequested?.Invoke(this, ViewType.Main);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowError("Fehler", "Ein Fehler ist aufgetreten beim Navigieren zurück zur Hauptansicht.");
            Logger.Error("Error navigating back: " + ex.Message);
        }
    }

    private void CustomerCombo_SelectionChanged(object? sender, RoutedEventArgs e)
    {
        if(_customerCombo.SelectedItem is not Customer customer || !_isEditing) return;
        _serviceeDatePicker.SelectedDate = customer.PaymentDueDate;
        _taxStatusCombo.SelectedItem = customer.NetCalculationType;
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.S || e.KeyModifiers != KeyModifiers.Control) return;
        e.Handled = true;
        _countOfOrdersOnLoad = _orders.Count;
        SaveOrderData();
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

    public void Dispose()
    {
        NavigationRequested = null;

        _orderListBox.SelectionChanged -= OrderListBox_SelectionChanged;

        _customerCombo.SelectionChanged -= CustomerCombo_SelectionChanged;

        _taxStatusCombo.SelectionChanged -= OnStatusChanged;

        _netAmountBox.TextChanged -= NetAmountBox_TextChanged;

        _newButton.Click -= NewButton_Click;
        _saveButton.Click -= SaveButton_Click;
        _deleteButton.Click -= DeleteButton_Click;
        _cancelButton.Click -= CancelButton_Click;

        (Content as Grid)?.Children.Clear();
        _orders.Clear();
        _customers.Clear();

        _orderListBox = null!;
        _editPanel = null!;
        _invoiceNumberBox = null!;
        _customerNumberBox = null!;
        _invoiceReferenceBox = null!;
        _routeFromBox = null!;
        _routeToBox = null!;
        _driverNameBox = null!;
        _driverLastNameBox = null!;
        _driverLicensePlateBox = null!;
        _driverPhoneBox = null!;
        _netAmountBox = null!;
        _descriptionBox = null!;
        _orderDatePicker = null!;
        _serviceeDatePicker = null!;
        _driverBirthdayPicker = null!;
        _customerCombo = null!;
        _freightTypeCombo = null!;
        _taxStatusCombo = null!;
        _podsCheckBox = null!;
        _taxAmountLabel = null!;
        _grossAmountLabel = null!;
        _saveButton = null!;
        _deleteButton = null!;
        _newButton = null!;
        _cancelButton = null!;
        _selectedOrder = null;

        KeyDown -= OnKeyDown;

        
        (Content as Grid)?.Children.Clear();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Logger.Log("OrderView disposed.");
    }
}