using AutoMapper;
using AutoSats.Data;

namespace AutoSats.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NewExchangeSchedule, ExchangeSchedule>()
            .ForMember(x => x.Spend, opts => opts.MapFrom(x => x.Amount.Amount))
            .ForMember(x => x.SpendCurrency, opts => opts.MapFrom(x => x.Amount.Symbol.Spend))
            .ForMember(x => x.Symbol, opts => opts.MapFrom(x => x.Amount.Symbol.Original));

        CreateMap<ExchangeSchedule, ExchangeScheduleSummary>();
    }
}
