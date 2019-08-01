using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using VCPKG;
using VCPKG.Message;
using VCPKG.CallBack;

namespace vcpkg_port_update_alert
{
    class Program
    {
        static void PrintHeaders()
        {
            VcpkgMessage.NormalMessage("vcpkg port update alert\n");
            VcpkgMessage.NormalMessage("Version 1.0\n");
            VcpkgMessage.NormalMessage("By Ehsan Mohammadi\n\n");
            VcpkgMessage.NormalMessage("Press any key to start checking vcpkg ports...");
        }

        static async Task AsyncMain()
        {
            // Initialize
            Vcpkg vcpkg = new Vcpkg();
            vcpkg.SetHttpClient();

            // Fetching ports
            VcpkgMessage.NormalMessage("\n\n> Fetching ports... (It may take a few seconds)\n");
            IEnumerable<string> ports = await vcpkg.GetPorts();

            if(ports != null)
            {
                VcpkgMessage.NormalMessage("> Ports successfully fetched\n");

                // Some settings for console output
                int currentLine = Console.CursorTop + 1;
                int clearLength = 0;
                int percent = 0;
                int portsCount = ports.ToList().Count;

                // Looking for need-to-be-updated ports
                foreach(string port in ports)
                {
                    percent++;
                    Console.SetCursorPosition(0, currentLine - 1);
                    VcpkgMessage.NormalMessage($"> Looking for need-to-be-updated ports... ({100 * percent / portsCount}%)");

                    // Reset the line
                    Console.SetCursorPosition(0, currentLine);
                    VcpkgMessage.ClearLine(currentLine, clearLength);

                    // Show the current port
                    Console.SetCursorPosition(0, currentLine);
                    VcpkgMessage.NormalMessage($"> Current port: \"{port}\"");
                    clearLength = 18 + port.Length;

                    // Get the port REPO and REF
                    RepoRef portRepoRef = await vcpkg.GetRepoAndRef(port);

                    if(vcpkg.HasRepoAndRef(portRepoRef))
                    {
                        string latestRelease = await vcpkg.SearchLatestRelease(portRepoRef.Repo);
                        
                        // Show need-to-be-update port
                        if(vcpkg.PortNeedToUpdate(portRepoRef.Ref, latestRelease))
                        {
                            Console.SetCursorPosition(0, currentLine - 1);
                            ++currentLine;
                            VcpkgMessage.WarningMessage($"\"{port}\" need to update from \"{portRepoRef.Ref}\" to \"{latestRelease}\" version.");
                            VcpkgMessage.NormalMessage("");
                        }
                    }
                }

                // Remove last port check in the currentLine
                Console.SetCursorPosition(0, currentLine);
                VcpkgMessage.ClearLine(currentLine, clearLength);
                Console.SetCursorPosition(0, currentLine);

                // Finish
                VcpkgMessage.NormalMessage("\n> All ports checked!\n");
                VcpkgMessage.NormalMessage("Press any key to quit...");
            }
        }

        static void Main(string[] args)
        {
            // Set console title and Show the header
            Console.Title = "vcpkg-port-update-alert - version: 1.0";
            PrintHeaders();
            Console.ReadKey();

            // Start vcpkg-port-update-alert
            AsyncMain().Wait();

            // Reset the color and quit
            Console.ReadKey();
            Console.ResetColor();
        }
    }
}
