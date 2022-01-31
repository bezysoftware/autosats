using AutoMapper;

namespace AutoSats.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NewExchangeSchedule, ExchangeSchedule>()
            .ForMember(x => x.Spend, opts => opts.MapFrom(x => x.Amount.Amount))
            .ForMember(x => x.SpendCurrency, opts => opts.MapFrom(x => x.Amount.Symbol.Spend))
            .ForMember(x => x.Symbol, opts => opts.MapFrom(x => x.Amount.Symbol.Original))
            .ForMember(x => x.Notification, opts => opts.MapFrom(x => x.Notification))
            .ForPath(x => x.Notification!.Type, opts => opts.MapFrom(x => x.NotificationType))
            .AfterMap((source, target) =>
            {
                if (source.NotificationType == NotificationType.None)
                {
                    target.Notification = null;
                }
            });

        CreateMap<ExchangeSchedule, ExchangeScheduleSummary>();
        CreateMap<NotificationSubscription, ExchangeScheduleNotification>();
    }
}
