namespace AutoSats.Models
{
    public enum WithdrawalType
    {
        /// <summary>
        /// Withdrawal isn't supported.
        /// </summary>
        None,

        /// <summary>
        /// Withdrawal is possible to an arbitrary address.
        /// </summary>
        Address,

        /// <summary>
        /// Withdrawal is possible only to a previously authorised addresses identified by a custom name.
        /// </summary>
        Name
    }
}
