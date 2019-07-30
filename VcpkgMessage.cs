using System;

namespace VCPKG.Message
{
    // A class for printing various messages
    public class VcpkgMessage
    {
        /// <summary>
        /// Print a normal message (White color)
        /// </summary> 
        public static void NormalMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message);
        }

        /// <summary>
        /// Print a warning message (Yellow color)
        /// </summary> 
        public static void WarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(message);
        }

        /// <summary>
        /// Print a error message (Red color)
        /// </summary> 
        public static void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
        }
    }
}