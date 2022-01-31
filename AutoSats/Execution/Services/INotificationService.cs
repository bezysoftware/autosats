namespace AutoSats.Execution.Services;

public interface INotificationService
{
    Task SendNotificationAsync(ExchangeEvent e);

    string GetServicePublicKey();
}
