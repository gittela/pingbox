using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Net.NetworkInformation;
using MicroLiquidCrystal;
using FusionWare.SPOT.Hardware;

namespace NetduinoPlusApplication1
{
    public class Program
    {
        static OutputPort ledpingred = new OutputPort(Pins.GPIO_PIN_D13, false);
        static OutputPort ledpinggreen = new OutputPort(Pins.GPIO_PIN_D12, false);
        static OutputPort leddhcpfail = new OutputPort(Pins.GPIO_PIN_D11, false);
        static OutputPort leddhcpsuccess = new OutputPort(Pins.GPIO_PIN_D10, false);
        //static InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        static Boolean _isNetworkOnline = true;

        static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _isNetworkOnline = e.IsAvailable;
        }


        public static void Main()
        {
            var bus = new I2CBus();
            var lcdProvider = new MCP23008LcdTransferProvider(bus, 0x0, MCP23008LcdTransferProvider.DefaultSetup);
            var lcd = new Lcd(lcdProvider);

            lcd.Begin(16, 2);
            lcd.Clear();
            lcd.Backlight = true;

            //lcd.SetCursorPosition(0, 1);

            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);

            NetworkInterface NI = NetworkInterface.GetAllNetworkInterfaces()[0];
            {

                //button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);
                {
                    //Debug.Print("buttonpress");
                    //NI.ReleaseDhcpLease();
                    //NI.RenewDhcpLease();
                }

                RawSocketPing pingSocket = null;

                IPAddress remoteAddress = IPAddress.Parse("195.159.0.100");

                int dataSize = 512, ttlValue = 128, sendCount = 1;

                while (true)
                {
                    bool ipok = (NI.IPAddress != "0.0.0.0");
                    lcd.SetCursorPosition(0, 0);
                    lcd.Write(NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
                    try
                    {
                        if (!_isNetworkOnline)
                        {
                            NetworkInterface[] iface = NetworkInterface.GetAllNetworkInterfaces();
                            iface[0].RenewDhcpLease();
                            lcd.Clear();
                        }
                        else
                        {

                            pingSocket = new RawSocketPing(ttlValue, dataSize, sendCount, 1337);

                            pingSocket.PingAddress = remoteAddress;

                            pingSocket.InitializeSocket();

                            pingSocket.BuildPingPacket();

                            bool success = pingSocket.DoPing();

                            lcd.SetCursorPosition(0, 1);
                            lcd.Write(success ? "Wohoo!" : "oh no!");

                            ledpinggreen.Write(success ? true : false);
                            ledpingred.Write(success ? false : true);
                            Debug.Print(success ? "Hey, we got a response!" : "No response");
                        }
                    }
                    catch (SocketException err)
                    {
                        //ledpingred.Write(true);
                        Debug.Print("Socket error occured: " + err.Message);
                    }

                    finally
                    {
                        if (pingSocket != null)
                            pingSocket.Close();
                    }
                }

            }
        }
    }
}