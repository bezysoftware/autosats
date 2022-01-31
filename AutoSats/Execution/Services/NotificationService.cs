using Newtonsoft.Json;
using WebPush;

namespace AutoSats.Execution.Services;

public class NotificationService : INotificationService
{
    private const string VapidSubject = "https://github.com/bezysoftware/autosats";
    private readonly ILogger<NotificationService> log;
    private readonly SatsContext db;

    public NotificationService(ILogger<NotificationService> log, SatsContext db)
    {
        this.log = log;
        this.db = db;
    }

    public string GetServicePublicKey()
    {
        var settings = this.db.ApplicationSettings.First();

        return settings.PublicKey;
    }

    public async Task SendNotificationAsync(ExchangeEvent e)
    {
        var notification = e.Schedule.Notification;

        if (notification == null || notification.Type == NotificationType.None)
        {
            // notification not setup
            return;
        }

        if (notification.Type == NotificationType.Errors && string.IsNullOrEmpty(e.Error))
        {
            return;
        }

        var settings = this.db.ApplicationSettings.First();
        var pushSubscription = new PushSubscription(notification.Url, notification.P256dh, notification.Auth);
        var vapidDetails = new VapidDetails(VapidSubject, settings.PublicKey, settings.PrivateKey);
        var webPushClient = new WebPushClient();

        try
        {
            var payload = JsonConvert.SerializeObject(new
            {
                message = GetNotificationText(e)
            });

            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception ex)
        {
            var json = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            this.log.LogError(ex, $"Error sending notification for event {JsonConvert.SerializeObject(e, json)}");
        }
    }

    private string GetNotificationText(ExchangeEvent e)
    {
        return e switch
        {
            ExchangeEventBuy buy when !string.IsNullOrEmpty(buy.Error) => $"Failed to buy {buy.Schedule.Spend} {buy.Schedule.SpendCurrency} worth of BTC on {buy.Schedule.Exchange}: {buy.Error}.",
            ExchangeEventBuy buy => $"Bought {buy.Received} BTC on {buy.Schedule.Exchange}.",
            ExchangeEventWithdrawal withdrawal when !string.IsNullOrEmpty(withdrawal.Error) => $"Failed to withdraw {withdrawal.Amount} BTC from {withdrawal.Schedule.Exchange} to {withdrawal.Address}: {withdrawal.Error}.",
            ExchangeEventWithdrawal withdrawal => $"Withdrew {withdrawal.Amount} BTC from {withdrawal.Schedule.Exchange} to {withdrawal.Address}.",
            _ => $"{e.Schedule.Exchange} schedule changed."
        };
    }
}
