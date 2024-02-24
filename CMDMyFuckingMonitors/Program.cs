//using PInvoke;
using System;
using System.Runtime.InteropServices;
using static LibMyFuckingMonitors.MonitorService;
namespace FixMyMonitors
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("Monitor Positioning Utility");
            Console.WriteLine("");
            Console.WriteLine("Commands:");
            Console.WriteLine("To get monitor info for all monitors: [Get]");
            Console.WriteLine("To set a monitor's offsets: [Set {monitorId} {xOffset} {yOffset}]");
            Console.WriteLine("To set a monitor as primary: [Primary {monitorId}]");
            Console.WriteLine("To get all monitor modes: [List {monitorId}]");
            Console.WriteLine("To exit utility: [Exit]");

            var command = "get";
            while (command != "exit")
            {
                var argz = command.Split(' ');
                switch (command.ToLower().Split(' ')[0])
                {
                    case "get":
                        var monitors = GetMonitorOffsets();
                        foreach (var mon in monitors)
                        {
                            var primaryStar = mon.Primary ? "*" : "";
                            Console.WriteLine($"Monitor: [{primaryStar}{mon.InternalId}]{mon.Description} @ X: {mon.Xpos} Y: {mon.Ypos} W: {mon.Width} H: {mon.Height} @:{mon.Frequency}hz");
                        }
                        break;
                    case "set":
                        if (argz.Length != 4)
                        {
                            Console.WriteLine("An invalid number of arguments were supplied for the set command! Use 'Get' to get monitor info");
                            break;
                        }
                        SetMonitorOffsets(uint.Parse(argz[1]), int.Parse(argz[2]), int.Parse(argz[3]));
                        break;
                    case "primary":
                        if (argz.Length != 2)
                        {
                            Console.WriteLine("An invalid number of arguments were supplied for the set command! Use 'Get' to get monitor info");
                            break;
                        }
                        SetAsPrimaryMonitor(uint.Parse(argz[1]));
                        break;
                    case "list":
                        if (argz.Length != 2)
                        {
                            Console.WriteLine("An invalid number of arguments were supplied for the list command! Use 'Get' to get monitor info");
                            break;
                        }
                        ListMonitorModes(uint.Parse(argz[1]));
                        break;
                    case "exit":
                        Console.WriteLine("Bye!");
                        break;
                    default:
                        break;
                }
                command = Console.ReadLine();
            }
        }
    }
}
