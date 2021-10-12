using Quartz;

namespace AutoSats.Execution
{
    public static class ExecutionConsts
    {
        public static readonly decimal FeeReserve = 0.00050000m;
        public static readonly string ScheduleKey = "ScheduleKey";
        public static readonly string KeysFileExtension = "keys";
        public static readonly JobKey ExchangeJobKey = new JobKey("ExchangeJob");
        public static readonly string[] StableCoins = new[] 
        {
            "BUSD",
            "DAI",
            "TUSD",
            "USDC",
            "USDT",
        };
    }
}
