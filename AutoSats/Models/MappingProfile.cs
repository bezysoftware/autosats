using AutoMapper;
using AutoSats.Data;

namespace AutoSats.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<NewExchangeSchedule, ExchangeSchedule>();
        }        
    }
}
