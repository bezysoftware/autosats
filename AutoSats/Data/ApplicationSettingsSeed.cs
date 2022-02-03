namespace AutoSats.Data;
public class ApplicationSettingsSeed
{
    private readonly SatsContext db;

    public ApplicationSettingsSeed(SatsContext db)
    {
        this.db = db;
    }

    public void Seed()
    {
        if (this.db.ApplicationSettings.Any())
        {
            return;
        }

        this.db.ApplicationSettings.Add(GenerateApplicationSettings());
        this.db.SaveChanges();
    }

    private ApplicationSettings GenerateApplicationSettings()
    {
        var keys = WebPush.VapidHelper.GenerateVapidKeys();

        return new ApplicationSettings
        {
            Id = 1,
            PrivateKey = keys.PrivateKey,
            PublicKey = keys.PublicKey
        };
    }
}
