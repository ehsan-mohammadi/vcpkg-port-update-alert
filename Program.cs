using System;
using VCPKG;

namespace vcpkg_port_update_alert
{
    class Program
    {
        static void PrintHeaders()
        {
            Console.WriteLine("vcpkg port update alert");
            Console.WriteLine("Version 1.0");
            Console.WriteLine("By Ehsan Mohammadi");
            Console.WriteLine();
            Console.WriteLine("Press any key to start checking ports...");
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
