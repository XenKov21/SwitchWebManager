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

        public string CabinetName { get; set; } = string.Empty;


        [BindProperty]
        public int[] SelectedPorts { get; set; } = Array.Empty<int>();

        [BindProperty]
        public bool Enable { get; set; }
        public void OnGet()
        {
            string community = "private";
           // string oidBase = "1.3.6.1.2.1.2.2.1.7"; // Admin status

            // Словарь IP → Номер кабинета
            var ipToCabinet = new Dictionary<string, string>
            {
                { "192.168.0.28", "28" },
                { "192.168.0.29", "29" },
                { "192.168.0.30", "30" },
                { "192.168.100.32", "32" },
                { "192.168.0.48", "48" }
             };


            CabinetName = ipToCabinet.ContainsKey(IpAddress) ? ipToCabinet[IpAddress] : IpAddress;

            PortModel = new PortModel
            {
                SwitchIp = IpAddress,
                Ports = new List<PortItem>()
            };

            try
            {
                using (UdpTarget target = new UdpTarget(IPAddress.Parse(IpAddress), 161, 2000, 1))
                {
                    OctetString communityStr = new OctetString(community);
                    AgentParameters param = new AgentParameters(communityStr) { Version = SnmpVersion.Ver2 };

                    for (int i = 1; i <= 18; i++) // Для 24 портов
                    {
                        string adminOid = $"1.3.6.1.2.1.2.2.1.7.{i}";
                        string operOid = $"1.3.6.1.2.1.2.2.1.8.{i}";

                        Pdu pdu = new Pdu(PduType.Get);
                        pdu.VbList.Add(adminOid);
                        pdu.VbList.Add(operOid);

                        SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, param);


                        if (response != null && response.Pdu.ErrorStatus == 0)
                        {
                            int adminStatus = Convert.ToInt32(((Integer32)response.Pdu.VbList[0].Value).Value);
                            int operStatus = Convert.ToInt32(((Integer32)response.Pdu.VbList[1].Value).Value);

                            bool isEnabled = adminStatus == 1;
                            bool isConnected = operStatus == 1;

                            // Показываем порт только если он подключён (оперативно активен)
                            
                                PortModel.Ports.Add(new PortItem
                                {
                                    PortNumber = i,
                                    IsEnabled = isEnabled,
                                    IsConnected  = operStatus == 1 // Добавим новое свойство
                                });

                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка получения состояния портов: {ex.Message}";
            }
        }

        //public void OnGet() 
        //{
        //    PortModel = new PortModel
        //    {
        //        SwitchIp = IpAddress,
        //        Ports = Enumerable.Range(1, 24).Select(i => new PortItem
        //        {
        //            PortNumber = i,
        //            IsSelected = false,
        //            IsEnabled = true // Пока заглушка — здесь можно добавить SNMP GET, если нужно узнать текущее состояние
        //        }).ToList()
        //    };
        //}

        public IActionResult OnPostTogglePort(int portNumber, bool enable)
        {
            try
            {
                SetPortState(portNumber, enable);
                TempData["Message"] = $"Порт {portNumber} успешно {(enable ? "включён" : "отключён")}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при изменении состояния порта {portNumber}: {ex.Message}";
            }

            return RedirectToPage("ManagePorts", new { ipAddress = IpAddress });
        }

        public IActionResult OnPostBulkToggle(string IpAddress, bool enable, List<int> SelectedPorts)
        {
            try
            {
                if (string.IsNullOrEmpty(IpAddress))
                {
                    TempData["Error"] = "IP-адрес не указан.";
                    return RedirectToPage("ManagePorts", new { ipAddress = IpAddress });
                }

                string community = "private";
                string oidBase = "1.3.6.1.2.1.2.2.1.7";

                using (UdpTarget target = new UdpTarget(IPAddress.Parse(IpAddress), 161, 2000, 1))
                {
                    OctetString communityStr = new OctetString(community);

                    foreach (var portNumber in SelectedPorts)
                    {
                        string fullOid = $"{oidBase}.{portNumber}";
                        int snmpValue = enable ? 1 : 2;

                        Pdu pdu = new Pdu(PduType.Set);
                        pdu.VbList.Add(new Oid(fullOid), new Integer32(snmpValue));

                        AgentParameters param = new AgentParameters(communityStr)
                        {
                            Version = SnmpVersion.Ver2
                        };

                        SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, param);

                        if (response == null || response.Pdu.ErrorStatus != 0)
                        {
                            TempData["Error"] = $"Ошибка при изменении состояния портов.";
                            return RedirectToPage("ManagePorts", new { ipAddress = IpAddress });
                        }
                    }

                    TempData["Message"] = $"Порты успешно {(enable ? "включены" : "отключены")}.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToPage("ManagePorts", new { ipAddress = IpAddress });
        }



        private void SetPortState(int portNumber, bool enable)
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

                if (response == null || response.Pdu.ErrorStatus != 0)
                {
                    throw new Exception($"SNMP ошибка для порта {portNumber}");
                }
            }
        }
    }
}
