using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            VcpkgMessage.NormalMessage("Press any key to start checking vcpkg ports...\n");
        }

        static async Task Main(string[] args)
        {
            PrintHeaders();
            Console.ReadKey();

            Vcpkg vcpkg = new Vcpkg();
            vcpkg.SetHttpClient();

            IEnumerable<string> ports = await vcpkg.GetPorts();

            foreach(string port in ports)
            {
                RepoRef currentPort = await vcpkg.GetPortFileCMake(port);
                
            }

            Console.ReadKey();
        }
    }
}
