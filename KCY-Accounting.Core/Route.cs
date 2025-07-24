namespace KCY_Accounting.Core;

public class Route(string from, string to)
{
    public readonly string From = from;
    public readonly string To = to;

    public override string ToString()
    {
        return $"{From} - {To}";
    }

    public static Route? ReadCsvLine(string line, bool skip = true)
    {
        var span = line.AsSpan();
        var sep = span.IndexOf(' ');
        try
        {
            if (sep < 0)
                throw new ArgumentException("Ungültiges Routenformat. Erwartet: 'Von Bis'.");

            var from = span[..sep].ToString();
            var to = span[(sep + 1)..].ToString();

            return new Route(from, to);
        }
        catch when (skip)
        {
            return null;
        }
    }
}