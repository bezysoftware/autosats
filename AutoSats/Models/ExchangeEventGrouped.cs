namespace AutoSats.Models;

public record ExchangeEventGrouped(ExchangeEvent Event)
{
    public DateTime Timestamp => Event.Timestamp;

    public ExchangeEventType Type => Event.Type;

    public ExchangeEventBuy? Buy => Event as ExchangeEventBuy;

    public ExchangeEventWithdrawal? Withdrawal => Event as ExchangeEventWithdrawal;

    public ExchangeEventCreate? Create => Event as ExchangeEventCreate;

    public ExchangeEventPause? Pause => Event as ExchangeEventPause;

    public ExchangeEventResume? Resume => Event as ExchangeEventResume;
}
