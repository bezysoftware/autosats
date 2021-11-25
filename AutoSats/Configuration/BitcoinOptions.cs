namespace AutoSats.Configuration;

public class BitcoinOptions
{
    public const string DefaultUrl = "http://localhost:8332";

    public BitcoinOptions()
    {
        Url = DefaultUrl;
    }

    public string Url { get; init; }

    public string? Auth { get; init; }
}
