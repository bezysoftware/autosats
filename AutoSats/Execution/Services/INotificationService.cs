namespace AutoSats.Execution.Services;

public interface INotificationService
{
    Task SendNotificationAsync(ExchangeEvent e);
    
    Task SendTestNotificationAsync(ExchangeScheduleNotification notification);

    string GetServicePublicKey();
}
