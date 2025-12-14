using Common.Logging.Configuration;
using Makaretu.Dns;
using MaterialDesignThemes.Wpf.Converters.CircularProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VRCFTtoVMCP.Osc;

namespace VRCFTtoVMCP {
    public class VRCFTtoVNyan {
        static VrcOscReceiver _receiver = new();
        static System.Timers.Timer _timer = new(1000);
        static Makaretu.Dns.ServiceProfile service;
        static Makaretu.Dns.ServiceDiscovery serviceDiscovery;
        static bool IsStop;

        static int VRCFTPort = 9001;
        static string VRCFTAddress = "127.0.0.1";

        public static void Log(byte[] message) {
            string StrMessage = "";
            foreach (byte b in message) {
                StrMessage += b;
            }
            Log(StrMessage);
        }
        public static void Log (string message) {
            Console.WriteLine (message);
        }
        
        public static void Start() {
            try {
                VRChat.CreateVRCAvatarFile("avtr_00000000-0000-0000-0000-000000000000.json");
                VRChat.CreateVRCAvatarFile("avtr_00000000-0000-0000-0000-000000000001.json");

                _receiver.Start(port: VRCFTPort);

                OscJsonServer.TrackingEnable = true;
                SendAvatarChange("avtr_00000000-0000-0000-0000-000000000001");

                _timer.AutoReset = true;
                _timer.Start();
                IsStop = false;
            } catch (Exception ex) {
                Log($"{DateTime.Now}: Start Failed. [{ex.Message}]");
                _receiver.Stop();
                _timer.Stop();
            }
        }

        static void Stop() {
            try {
                OscJsonServer.TrackingEnable = false;
                SendAvatarChange("avtr_00000000-0000-0000-0000-000000000000");
            } catch (Exception) {
                //
            }
            _receiver.Stop();
            _timer.Stop();
            IsStop = true;
        }

        static async void SendAvatarChange(string id) {
            using OscClient oc1 = new(VRCFTAddress, VRCFTPort);
            oc1.Send(new Osc.Message("/avatar/change", id));

            IReadOnlyList<Zeroconf.IZeroconfHost> results = await Zeroconf.ZeroconfResolver.ResolveAsync("_osc._udp.local.");
            foreach (var result in results) {
                foreach (var service in result.Services) {
                    System.Diagnostics.Debug.WriteLine($"{service.Key} {service.Value.Port}");
                    if (service.Key.StartsWith("VRCFT-")) {
                        using OscClient oc2 = new(VRCFTAddress, service.Value.Port);
                        oc2.Send(new Osc.Message("/avatar/change", id));
                    }
                }
            }
        }

        static void Init() {
            int port = OscJsonServer.Start(61891);
            service = new Makaretu.Dns.ServiceProfile($"VRChat-Client-{new Random().Next(100000, 1000000)}", "_oscjson._tcp", (ushort)port, new IPAddress[] { IPAddress.Loopback });
            serviceDiscovery = new Makaretu.Dns.ServiceDiscovery();
            serviceDiscovery.Advertise(service);

            // Announce() だけだと VRCFT が反応しない。
            // Unadvertise() で Goodbyeパケット投げると VRCFT がなぜか反応するのでとりあえずこの実装としておく。
            serviceDiscovery.Unadvertise(service);
            serviceDiscovery.Advertise(service);
            serviceDiscovery.Announce(service);
        }

        static void Cleanup() {
            if (!IsStop) {
                Stop();
            }

            serviceDiscovery.Unadvertise(service);
            serviceDiscovery.Dispose();
            OscJsonServer.Stop();
        }

        static void Main () {
            Log("Press Enter innit");
            Console.ReadLine();
            Init();
            Log("Press Enter to turn avatar on");
            Console.ReadLine();
            Start();
            Log("Started. Press Enter to turn avatar off");
            Console.ReadLine();
            Stop();
            Log("Stopped. Press Enter for cleanup");
            Console.ReadLine();
            Cleanup();
        }
    }
    internal class DynamicSharedParameter {
        private static readonly object _LockObject = new();
        private static bool _EyeTargetPositionUse = true;
        private static int _EyeTargetPositionMultiplierUp = 100;
        private static int _EyeTargetPositionMultiplierDown = 100;
        private static int _EyeTargetPositionMultiplierLeft = 100;
        private static int _EyeTargetPositionMultiplierRight = 100;

        public static bool EyeTargetPositionUse {
            get {
                bool ret;
                lock (_LockObject) {
                    ret = _EyeTargetPositionUse;
                }
                return ret;
            }
            set {
                lock (_LockObject) {
                    _EyeTargetPositionUse = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierUp {
            get {
                int ret;
                lock (_LockObject) {
                    ret = _EyeTargetPositionMultiplierUp;
                }
                return ret;
            }
            set {
                lock (_LockObject) {
                    _EyeTargetPositionMultiplierUp = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierDown {
            get {
                int ret;
                lock (_LockObject) {
                    ret = _EyeTargetPositionMultiplierDown;
                }
                return ret;
            }
            set {
                lock (_LockObject) {
                    _EyeTargetPositionMultiplierDown = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierLeft {
            get {
                int ret;
                lock (_LockObject) {
                    ret = _EyeTargetPositionMultiplierLeft;
                }
                return ret;
            }
            set {
                lock (_LockObject) {
                    _EyeTargetPositionMultiplierLeft = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierRight {
            get {
                int ret;
                lock (_LockObject) {
                    ret = _EyeTargetPositionMultiplierRight;
                }
                return ret;
            }
            set {
                lock (_LockObject) {
                    _EyeTargetPositionMultiplierRight = value;
                }
            }
        }
    }

    internal static class MessageCount {
        private static readonly object _LockObjectVRCFT2ThisApp = new();
        private static readonly object _LockObjectThisApp2VRCFT = new();
        private static readonly object _LockObjectThisApp2VMC = new();
        private static int _CountVRCFT2ThisApp = 0;
        private static int _CountThisApp2VRCFT = 0;
        private static int _CountThisApp2VMC = 0;

        public static int CountUpVRCFT2ThisApp() {
            int count;
            lock (_LockObjectVRCFT2ThisApp) {
                _CountVRCFT2ThisApp++;
                count = _CountVRCFT2ThisApp;
            }
            return count;
        }

        public static int CountUpThisApp2VRCFT() {
            int count;
            lock (_LockObjectThisApp2VRCFT) {
                _CountThisApp2VRCFT++;
                count = _CountThisApp2VRCFT;
            }
            return count;
        }

        public static int CountUpThisApp2VMC() {
            int count;
            lock (_LockObjectThisApp2VMC) {
                _CountThisApp2VMC++;
                count = _CountThisApp2VMC;
            }
            return count;
        }

        public static int CountClearVRCFT2ThisApp() {
            int count = 0;
            lock (_LockObjectVRCFT2ThisApp) {
                count = _CountVRCFT2ThisApp;
                _CountVRCFT2ThisApp = 0;
            }
            return count;
        }

        public static int CountClearThisApp2VRCFT() {
            int count = 0;
            lock (_LockObjectThisApp2VRCFT) {
                count = _CountThisApp2VRCFT;
                _CountThisApp2VRCFT = 0;
            }
            return count;
        }

        public static int CountClearThisApp2VMC() {
            int count = 0;
            lock (_LockObjectThisApp2VMC) {
                count = _CountThisApp2VMC;
                _CountThisApp2VMC = 0;
            }
            return count;
        }

        public static void CountClearAll() {
            _CountVRCFT2ThisApp = 0;
            _CountThisApp2VRCFT = 0;
            _CountThisApp2VMC = 0;
        }
    }
}
