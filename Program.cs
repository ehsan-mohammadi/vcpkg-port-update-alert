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

                            string updatePort = $"\"{port}\" need to update from \"{portRepoRef.Ref}\" to \"{latestRelease}\" version.";
                            
                            VcpkgMessage.WarningMessage(updatePort);
                            VcpkgMessage.NormalMessage("");

                            // Add port to the log list
                            vcpkg.AddToLog(updatePort);
                        }
                    }
                }

                // Remove last port check in the currentLine
                Console.SetCursorPosition(0, currentLine);
                VcpkgMessage.ClearLine(currentLine, clearLength);
                Console.SetCursorPosition(0, currentLine);

                // Finish
                VcpkgMessage.NormalMessage("\n> All ports checked!\n\n");
            }
        }

        static void SaveLog()
        {
            // Reset the color
            Console.ResetColor();

            Vcpkg vcpkg = new Vcpkg();
            
            // Check the length of log list
            if(vcpkg.LogLength() > 0)
            {
                while(true)
                {
                    VcpkgMessage.NormalMessage("> Do you want to save ports in a log file? (y/n) ");
                    string answer = Console.ReadLine();

                    if(answer.ToLower() == "y")
                    {
                        VcpkgMessage.NormalMessage("> Enter the destination file path: ");
                        string path = Console.ReadLine();

                        if(vcpkg.SaveLog(path))
                        {
                            VcpkgMessage.NormalMessage("\n> File successfully saved!\n\n");
                            break;
                        }
                        else
                        {
                            VcpkgMessage.ErrorMessage("\n> Can't save the file. try again... (See EXAMPLE: C:\\ports.log)\n\n");
                            Console.ResetColor();
                        }
                    }
                    else if(answer.ToLower() == "n")
                    {
                        break;
                    }
                }
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

            // wait until you type "y" or "n"
            SaveLog();

            // Reset the color and quit
            VcpkgMessage.NormalMessage("Press any key to quit...");
            Console.ReadKey();
            Console.ResetColor();
        }
    }
}
