namespace SwitchWebManager.Models
{
    public class SwitchModel
    {
        public string IpAddress { get; set; } = string.Empty;
        // Или можно сделать свойство nullable:
        // public string? IpAddress { get; set; }
    }
}