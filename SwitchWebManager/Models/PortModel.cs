using System.ComponentModel.DataAnnotations;

namespace SwitchWebManager.Models
{
    public class PortModel
    {
        [Required]
        public string SwitchIp { get; set; } = string.Empty;

        public List<PortItem> Ports { get; set; } = new List<PortItem>();
    }

    public class PortItem
    {
        public int PortNumber { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsConnected { get; set; }
    }
}