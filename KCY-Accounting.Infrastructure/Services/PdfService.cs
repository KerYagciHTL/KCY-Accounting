using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KCY_Accounting.Infrastructure.Services;

/// <summary>
/// Generates professional Austrian invoices (Rechnungen) as PDF files using QuestPDF.
/// The layout follows the Austrian invoicing standard (§ 11 UStG):
///   – Rechnungsaussteller (Absender)
///   – Rechnungsempfänger (Kunde)
///   – Fortlaufende Rechnungsnummer
///   – Rechnungsdatum &amp; Lieferdatum
///   – Leistungsbeschreibung, Menge, Einzelpreis, Steuersatz
///   – Nettobetrag, MwSt-Betrag, Bruttobetrag
///   – UID-Nummer des Ausstellers
/// </summary>
public class PdfService : IPdfService
{
    // ── Company / issuer data – adjust these once for your business ──────────
    private static class Issuer
    {
        public const string Name       = "KCY Accounting & Spedition GmbH";
        public const string Street     = "Musterstraße 1";
        public const string ZipCity    = "1010 Wien";
        public const string Country    = "Österreich";
        public const string Phone      = "+43 1 000 0000";
        public const string Email      = "office@kcy-accounting.at";
        public const string Uid        = "ATU00000000";
        public const string Iban       = "AT00 0000 0000 0000 0000";
        public const string Bic        = "XXXXXXXX";
        public const string BankName   = "Musterbank AG";
    }

    // ── Brand colours ────────────────────────────────────────────────────────
    private static readonly string AccentBlue  = "#1E3A5F";
    private static readonly string LightBlue   = "#EBF2FA";
    private static readonly string MidGray     = "#6B7280";
    private static readonly string BorderGray  = "#E5E7EB";
    private static readonly string TableHeader = "#1E3A5F";

    public Task<string> GenerateInvoicePdfAsync(Invoice invoice, TransportOrder order, Customer customer)
    {
        // Community licence – free for non-commercial / small business use
        QuestPDF.Settings.License = LicenseType.Community;

        var outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Documents", "KCY-Rechnungen");
        Directory.CreateDirectory(outputDir);

        var fileName  = $"{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath  = Path.Combine(outputDir, fileName);

        // Pre-calculate amounts once so we can reuse them in the layout
        var net       = invoice.Amount;
        var vatAmount = Math.Round(net * invoice.VatRate / 100m, 2);
        var gross     = net + vatAmount;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(t => t.FontFamily("Helvetica").FontSize(9).FontColor("#111827"));

                // ── Header band ───────────────────────────────────────────────
                page.Header().Element(ComposeHeader);

                // ── Main content ──────────────────────────────────────────────
                page.Content().PaddingHorizontal(40).PaddingVertical(20).Column(col =>
                {
                    col.Spacing(16);

                    // Addresses row
                    col.Item().Row(row =>
                    {
                        // Sender (small, top-left – for window envelope)
                        row.RelativeItem(5).Column(sender =>
                        {
                            sender.Item()
                                  .Text($"{Issuer.Name}  ·  {Issuer.Street}  ·  {Issuer.ZipCity}")
                                  .FontSize(7).FontColor(MidGray);

                            sender.Item().Height(8);

                            // Recipient
                            sender.Item().Text(customer.CompanyName).Bold().FontSize(11);
                            if (!string.IsNullOrWhiteSpace(customer.ContactPerson))
                                sender.Item().Text($"z.H. {customer.ContactPerson}").FontSize(9);
                            sender.Item().Text(customer.Street);
                            sender.Item().Text($"{customer.ZipCode} {customer.City}");
                            sender.Item().Text(customer.Country);
                            if (!string.IsNullOrWhiteSpace(customer.VatNumber))
                                sender.Item().Text($"UID: {customer.VatNumber}").FontColor(MidGray);
                        });

                        row.ConstantItem(10); // spacer

                        // Invoice meta table (right column)
                        row.RelativeItem(4).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(5);
                                c.RelativeColumn(5);
                            });

                            void MetaRow(string label, string value)
                            {
                                t.Cell().Text(label).FontColor(MidGray);
                                t.Cell().AlignRight().Text(value).Bold();
                            }

                            MetaRow("Rechnungsnummer:", invoice.InvoiceNumber);
                            MetaRow("Rechnungsdatum:",  invoice.IssuedAt.ToString("dd.MM.yyyy"));
                            MetaRow("Zahlungsziel:",    invoice.DueDate.ToString("dd.MM.yyyy"));
                            MetaRow("Auftragsnummer:",  order.OrderNumber);
                            if (!string.IsNullOrWhiteSpace(order.CustomerReference))
                                MetaRow("Ihre Referenz:", order.CustomerReference);
                            MetaRow("Währung:", invoice.Currency);
                        });
                    });

                    // ── Heading ───────────────────────────────────────────────
                    col.Item().PaddingTop(8).Text(t =>
                    {
                        t.Span("RECHNUNG  ").FontSize(18).Bold().FontColor(AccentBlue);
                        t.Span(invoice.InvoiceNumber).FontSize(14).FontColor(MidGray);
                    });

                    // ── Line items table ──────────────────────────────────────
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(28);   // pos
                            c.RelativeColumn(6);    // description
                            c.RelativeColumn(2);    // qty
                            c.RelativeColumn(2);    // unit
                            c.RelativeColumn(2);    // unit price
                            c.RelativeColumn(1.5f); // vat%
                            c.RelativeColumn(2);    // total
                        });

                        // Header row
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(TableHeader).Padding(6);

                        table.Header(h =>
                        {
                            void Hdr(string txt, bool right = false)
                            {
                                var cell = h.Cell().Element(HeaderCell);
                                var text = cell.Text(txt).FontColor(Colors.White).Bold().FontSize(8.5f);
                                if (right) text.AlignRight();
                            }

                            Hdr("Pos.");
                            Hdr("Bezeichnung");
                            Hdr("Menge", true);
                            Hdr("Einheit");
                            Hdr("Einzelpreis", true);
                            Hdr("MwSt%", true);
                            Hdr("Gesamtpreis", true);
                        });

                        // Single line item (transport service)
                        static IContainer DataCell(IContainer c) =>
                            c.BorderBottom(1).BorderColor(BorderGray).Padding(6);

                        table.Cell().Element(DataCell).Text("1");
                        table.Cell().Element(DataCell).Column(dc =>
                        {
                            dc.Item().Text(BuildServiceDescription(order)).Bold();
                            dc.Item().Text($"Auftrag {order.OrderNumber}").FontColor(MidGray).FontSize(8);
                            if (!string.IsNullOrWhiteSpace(order.GoodsDescription))
                                dc.Item().Text(order.GoodsDescription).FontColor(MidGray).FontSize(8);
                            var route = BuildRoute(order);
                            if (!string.IsNullOrWhiteSpace(route))
                                dc.Item().Text(route).FontColor(MidGray).FontSize(8);
                        });
                        table.Cell().Element(DataCell).AlignRight().Text("1");
                        table.Cell().Element(DataCell).Text("Pauschale");
                        table.Cell().Element(DataCell).AlignRight().Text($"{net:N2} {invoice.Currency}").Bold();
                        table.Cell().Element(DataCell).AlignRight().Text($"{invoice.VatRate:N0}%");
                        table.Cell().Element(DataCell).AlignRight().Text($"{net:N2} {invoice.Currency}").Bold();
                    });

                    // ── Totals ────────────────────────────────────────────────
                    col.Item().AlignRight().Width(240).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(3);
                        });

                        static IContainer TotalCell(IContainer c) =>
                            c.BorderBottom(1).BorderColor(BorderGray).Padding(5);

                        void TotalRow(string label, string value, bool highlight = false)
                        {
                            if (highlight)
                            {
                                t.Cell().Background(AccentBlue).Padding(7)
                                 .Text(label).FontColor(Colors.White).Bold().FontSize(10);
                                t.Cell().Background(AccentBlue).Padding(7).AlignRight()
                                 .Text(value).FontColor(Colors.White).Bold().FontSize(10);
                            }
                            else
                            {
                                t.Cell().Element(TotalCell).Text(label).FontColor(MidGray);
                                t.Cell().Element(TotalCell).AlignRight().Text(value);
                            }
                        }

                        TotalRow("Nettobetrag:",
                            $"{net:N2} {invoice.Currency}");
                        TotalRow($"MwSt ({invoice.VatRate:N0}%):",
                            $"{vatAmount:N2} {invoice.Currency}");
                        TotalRow("GESAMTBETRAG:",
                            $"{gross:N2} {invoice.Currency}", highlight: true);
                    });

                    // ── Payment note ──────────────────────────────────────────
                    col.Item().Background(LightBlue).Padding(12).Column(note =>
                    {
                        note.Item().Text("Zahlungsinformationen").Bold().FontColor(AccentBlue);
                        note.Item().Height(4);
                        note.Item().Text(t =>
                        {
                            t.Span("Bitte überweisen Sie den Betrag von ");
                            t.Span($"{gross:N2} {invoice.Currency}").Bold();
                            t.Span($" bis zum ");
                            t.Span(invoice.DueDate.ToString("dd.MM.yyyy")).Bold();
                            t.Span(" auf folgendes Konto:");
                        });
                        note.Item().Height(6);
                        note.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Bank: {Issuer.BankName}");
                                c.Item().Text($"IBAN: {Issuer.Iban}");
                                c.Item().Text($"BIC:  {Issuer.Bic}");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Verwendungszweck:").Bold();
                                c.Item().Text(invoice.InvoiceNumber);
                            });
                        });
                    });

                    // ── Notes ─────────────────────────────────────────────────
                    col.Item().Text("Bei Fragen zu dieser Rechnung wenden Sie sich bitte an " +
                                    $"{Issuer.Phone} oder {Issuer.Email}.")
                              .FontColor(MidGray).FontSize(8);
                });

                // ── Footer ────────────────────────────────────────────────────
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    private static void ComposeHeader(IContainer container)
    {
        container.Background(AccentBlue).Padding(0).Row(row =>
        {
            // Company name / logo area
            row.RelativeItem().PaddingHorizontal(40).PaddingVertical(20).Column(col =>
            {
                col.Item().Text(Issuer.Name).FontColor(Colors.White).Bold().FontSize(16);
                col.Item().Text($"{Issuer.Street}  ·  {Issuer.ZipCity}  ·  {Issuer.Country}")
                          .FontColor("#A5C8E8").FontSize(8);
            });

            // Contact info (right side of header)
            row.ConstantItem(200).PaddingHorizontal(20).PaddingVertical(20).Column(col =>
            {
                col.Item().AlignRight().Text(Issuer.Phone).FontColor("#A5C8E8").FontSize(8);
                col.Item().AlignRight().Text(Issuer.Email).FontColor("#A5C8E8").FontSize(8);
                col.Item().AlignRight().Text($"UID: {Issuer.Uid}").FontColor("#A5C8E8").FontSize(8).Bold();
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Background(AccentBlue).Height(28).PaddingHorizontal(40)
                 .Row(row =>
                 {
                     row.RelativeItem().AlignMiddle()
                        .Text($"{Issuer.Name}  ·  UID {Issuer.Uid}  ·  {Issuer.Iban}")
                        .FontColor("#A5C8E8").FontSize(7);

                     row.ConstantItem(80).AlignMiddle().AlignRight()
                        .Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontColor("#A5C8E8").FontSize(7));
                            t.Span("Seite ");
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                 });
    }

    private static string BuildServiceDescription(TransportOrder order)
    {
        return order.FreightType switch
        {
            Core.Models.FreightType.Ftl                      => "Transportleistung – Komplettladung (FTL)",
            Core.Models.FreightType.EuroPalletWithExchange   => "Transportleistung – EUR-Paletten mit Tausch",
            Core.Models.FreightType.EuroPalletWithoutExchange => "Transportleistung – EUR-Paletten ohne Tausch",
            _ => "Transportleistung"
        };
    }

    private static string BuildRoute(TransportOrder order)
    {
        var from = $"{order.LoadingPoint.City} ({order.LoadingPoint.Country})";
        var to   = $"{order.UnloadingPoint.City} ({order.UnloadingPoint.Country})";
        return $"Route: {from}  →  {to}";
    }
}

