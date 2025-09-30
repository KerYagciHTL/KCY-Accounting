using PdfSharp.Fonts;

namespace KCY_Accounting.Core;

public class DefaultFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        switch (faceName.ToLowerInvariant())
        {
            case "arial":
            case "arial bold":
            case "arial italic":
            case "arial bold italic":
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (File.Exists(fontPath))
                    return File.ReadAllBytes(fontPath);
                break;
        }
        
        throw new FileNotFoundException($"Font '{faceName}' not found.");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
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