using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SnmpSharpNet;
using System.Net;
using SwitchWebManager.Models;

namespace SwitchWebManager.Pages
{
    public class ManagePortsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string IpAddress { get; set; } = string.Empty;

        [BindProperty]
        public PortModel PortModel { get; set; } = new PortModel();

        public void OnGet()
        {
            PortModel = new PortModel
            {
                SwitchIp = IpAddress,
                Ports = Enumerable.Range(1, 24).Select(i => new PortItem
                {
                    PortNumber = i,
                    IsSelected = false,
                    IsEnabled = true
                }).ToList()
            };
        }

        public IActionResult OnPostTogglePort(int portNumber, bool enable)
        {
            try
            {
                string community = "private";
                string oidBase = "1.3.6.1.2.1.2.2.1.7";
                string fullOid = $"{oidBase}.{portNumber}";
                int snmpValue = enable ? 1 : 2;

                using (UdpTarget target = new UdpTarget(IPAddress.Parse(IpAddress), 161, 2000, 1))
                {
                    OctetString communityStr = new OctetString(community);

                    Pdu pdu = new Pdu(PduType.Set);
                    pdu.VbList.Add(new Oid(fullOid), new Integer32(snmpValue));

                    AgentParameters param = new AgentParameters(communityStr)
                    {
                        Version = SnmpVersion.Ver2
                    };

                    SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, param);

                    if (response != null && response.Pdu.ErrorStatus == 0)
                    {
                        TempData["Message"] = $"Порт {portNumber} успешно {(enable ? "включён" : "отключён")}.";
                    }
                    else
                    {
                        TempData["Error"] = $"Ошибка при изменении состояния порта {portNumber}.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToPage("ManagePorts", new { ipAddress = IpAddress });
        }
    }
}