using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Shared
{
    public class Logger
    {
        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }

        public static void LogError(string message, Exception? ex = null)
        {
            // 這個不加 [Conditional]，代表不論 Debug / Release 都會執行
            Console.Error.WriteLine($"[ERROR] {message}");
            if (ex != null)
                Console.Error.WriteLine(ex.ToString());
        }

        public static void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }
    }
}
