using System.Runtime.InteropServices;
using static LibMyFuckingMonitors.DevMode;
using static LibMyFuckingMonitors.DisplayDevice;

namespace LibMyFuckingMonitors
{
    public static class MonitorService
    {
        public static void SetMonitorOffsets(uint monitorId, int x, int y)
        {
            var device = new DISPLAY_DEVICE();
            var deviceMode = new DEVMODE();
            device.cb = Marshal.SizeOf(device);

            NativeMethods.EnumDisplayDevices(null, monitorId, ref device, 0);
            NativeMethods.EnumDisplaySettings(device.DeviceName, -1, ref deviceMode);
            var offsetx = deviceMode.dmPosition.x;
            var offsety = deviceMode.dmPosition.y;
            Console.WriteLine($"Current x: {offsetx} - New x: {x}");
            Console.WriteLine($"Current y: {offsety} - New y: {y}");
            deviceMode.dmPosition.x = x;
            deviceMode.dmPosition.y = y;

            NativeMethods.ChangeDisplaySettingsEx(
                device.DeviceName,
                ref deviceMode,
                (IntPtr)null,
                (ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET),
                IntPtr.Zero);

            device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);
            // Apply settings
            NativeMethods.ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
        }

        public static void SetMonitorMode(uint monitorId, MonitorModes mode)
        {
            var device = new DISPLAY_DEVICE();
            var deviceMode = new DEVMODE();
            device.cb = Marshal.SizeOf(device);

            NativeMethods.EnumDisplayDevices(null, monitorId, ref device, 0);
            NativeMethods.EnumDisplaySettings(device.DeviceName, -1, ref deviceMode);
            
            deviceMode.dmPelsWidth = mode.Width;
            deviceMode.dmPelsHeight = mode.Height;
            deviceMode.dmDisplayFrequency = mode.Frequency;
            deviceMode.dmBitsPerPel = mode.ColorBits;

            NativeMethods.ChangeDisplaySettingsEx(
                device.DeviceName,
                ref deviceMode,
                (IntPtr)null,
                (ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET),
                IntPtr.Zero);

            device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);
            // Apply settings
            NativeMethods.ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
        }

        public static List<MonitorModel> GetMonitorOffsets()
        {
            var monitors = new List<MonitorModel>();
            var frindlyNames = Tools.GetAllMonitorsFriendlyNames().ToArray();
            var device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);
            var monIndex = 0;
            for (uint otherid = 0; NativeMethods.EnumDisplayDevices(null, otherid, ref device, 0); otherid++)
            {
                if (device.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                {
                    device.cb = Marshal.SizeOf(device);
                    var otherDeviceMode = new DEVMODE();

                    NativeMethods.EnumDisplaySettings(device.DeviceName, -1, ref otherDeviceMode);

                    DEVMODE vDevMode = new DEVMODE();
                    int i = 0;
                    List<MonitorModes> monitorModes = new();
                    while (NativeMethods.EnumDisplaySettings(device.DeviceName, i, ref vDevMode))
                    {
                        var thisMode = new MonitorModes()
                        {
                            Width = vDevMode.dmPelsWidth,
                            Height = vDevMode.dmPelsHeight,
                            ColorBits = vDevMode.dmBitsPerPel,
                            Frequency = vDevMode.dmDisplayFrequency
                        };
                        if(!monitorModes.Any(x => 
                            x.ColorBits == thisMode.ColorBits && 
                            x.Width == thisMode.Width && 
                            x.Height == thisMode.Height && 
                            x.Frequency == thisMode.Frequency))
                                monitorModes.Add(thisMode);
                        i++;
                    }
                    monitors.Add(new MonitorModel()
                    {
                        MonNum = (int)monIndex,
                        InternalId = otherid,
                        Xpos = otherDeviceMode.dmPosition.x,
                        Ypos = otherDeviceMode.dmPosition.y,
                        Frequency = otherDeviceMode.dmDisplayFrequency,
                        Height = otherDeviceMode.dmPelsHeight,
                        Width = otherDeviceMode.dmPelsWidth,
                        ColorBits = otherDeviceMode.dmBitsPerPel,
                        Name = otherDeviceMode.dmDeviceName,
                        Description = frindlyNames[monIndex],
                        GPU = device.DeviceString,
                        Primary = device.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice),
                        Modes = monitorModes,
                    }); ;
                    monIndex++;
                }
            }
            return monitors;
        }

        public static void ListMonitorModes(uint monitorId)
        {
            var monitor = GetMonitorOffsets().FirstOrDefault(x => x.MonNum == monitorId);

            foreach (var mode in monitor.Modes)
            {
                Console.WriteLine("Name: {0} Width:{1} Height:{2} Color:{3} Frequency:{4}",
                                        monitor.Description,
                                        mode.Width,
                                        mode.Height,
                                        mode.ColorBits,
                                        mode.Frequency
                                    );
            }
        }

        public static void SetAsPrimaryMonitor(uint id)
        {
            var device = new DISPLAY_DEVICE();
            var deviceMode = new DEVMODE();
            device.cb = Marshal.SizeOf(device);

            NativeMethods.EnumDisplayDevices(null, id, ref device, 0);
            NativeMethods.EnumDisplaySettings(device.DeviceName, -1, ref deviceMode);
            var offsetx = deviceMode.dmPosition.x;
            var offsety = deviceMode.dmPosition.y;
            Console.WriteLine($"offsetx: {offsetx}");
            Console.WriteLine($"offsety: {offsety}");
            deviceMode.dmPosition.x = 0;
            deviceMode.dmPosition.y = 0;

            NativeMethods.ChangeDisplaySettingsEx(
                device.DeviceName,
                ref deviceMode,
                (IntPtr)null,
                (ChangeDisplaySettingsFlags.CDS_SET_PRIMARY | ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET),
                IntPtr.Zero);

            device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            // Update remaining devices
            for (uint otherid = 0; NativeMethods.EnumDisplayDevices(null, otherid, ref device, 0); otherid++)
            {
                if (device.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop) && otherid != id)
                {
                    device.cb = Marshal.SizeOf(device);
                    var otherDeviceMode = new DEVMODE();

                    NativeMethods.EnumDisplaySettings(device.DeviceName, -1, ref otherDeviceMode);
                    Console.WriteLine($"Monitor: {otherid} @ X: {otherDeviceMode.dmPosition.x} Y: {otherDeviceMode.dmPosition.y}");
                    otherDeviceMode.dmPosition.x -= offsetx;
                    otherDeviceMode.dmPosition.y -= offsety;

                    NativeMethods.ChangeDisplaySettingsEx(
                        device.DeviceName,
                        ref otherDeviceMode,
                        (IntPtr)null,
                        (ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET),
                        IntPtr.Zero);

                }

                device.cb = Marshal.SizeOf(device);
            }

            // Apply settings
            NativeMethods.ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
        }
    }
}
