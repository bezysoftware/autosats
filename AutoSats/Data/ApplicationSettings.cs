using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data;

public class ApplicationSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string PublicKey { get; set; } = string.Empty;

    [Required]
    public string PrivateKey { get; set; } = string.Empty;
}
