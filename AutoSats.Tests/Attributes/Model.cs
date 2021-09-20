using AutoSats.Validation;

namespace AutoSats.Tests.Attributes
{
    public class Model
    {
        [BitcoinAddress]
        public string Address { get; set; }
    }
}
