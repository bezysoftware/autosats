using AutoMapper;
using AutoSats.Data;
using System.Collections.Generic;

namespace AutoSats.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<NewExchangeSchedule, ExchangeSchedule>();
            CreateMap<ExchangeSchedule, ExchangeScheduleSummary>();
        }        
    }
}
