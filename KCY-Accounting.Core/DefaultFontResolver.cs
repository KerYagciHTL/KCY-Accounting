using PdfSharp.Fonts;

namespace KCY_Accounting.Core;

// Standard Font Resolver für PDFsharp
public class DefaultFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        // Versuche Standard-Windows-Fonts zu verwenden
        switch (faceName.ToLowerInvariant())
        {
            case "arial":
            case "arial bold":
            case "arial italic":
            case "arial bold italic":
                // Für Windows-Systeme
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (File.Exists(fontPath))
                    return File.ReadAllBytes(fontPath);
                break;
        }
        
        // Fallback: Verwende einen eingebetteten Font oder wirf eine Exception
        throw new FileNotFoundException($"Font '{faceName}' not found.");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Vereinfachte Implementierung
        var fontName = familyName.ToLowerInvariant();
        if (isBold && isItalic)
            fontName += " bold italic";
        else if (isBold)
            fontName += " bold";
        else if (isItalic)
            fontName += " italic";
        
        return new FontResolverInfo(fontName);
    }
}