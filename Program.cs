using System;
using VCPKG;
using VCPKG.Message;

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

        static void Main(string[] args)
        {
            PrintHeaders();

            Vcpkg vcpkg = new Vcpkg();
            vcpkg.GetVcpkgRepo();
            Console.ReadKey();
        }
    }
}
