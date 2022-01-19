using CsvHelper.Configuration;

namespace AutoSats.Models;

public class ExchangeEventMap : ClassMap<BuyWithdrawal>
{
    public ExchangeEventMap()
    {
        Map(x => x.Timestamp).TypeConverterOption.Format("o").Index(0);
        Map(x => x.Type).Index(1);
        Map(x => x.Buy!.OrderId).Index(2);
        Map(x => x.Buy!.Price).Index(3);
        Map(x => x.Buy!.Received).Index(4);
        Map(x => x.Withdrawal!.WithdrawalId).Index(5);
        Map(x => x.Withdrawal!.Address).Index(6).Name("WithdrawalAddress");
        Map(x => x.Withdrawal!.Amount).Index(7).Name("WithdrawalAmount");
    }
}