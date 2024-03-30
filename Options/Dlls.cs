using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace NetCoreSvchost.Options
{
    public class Dll
    {
        public string? FileName { get; set; }

        public string[]? Args { get; set; }
    }

    public class DllList
    {
        public static nint Add(Dll dll)
        {
            if (!NativeLibrary.TryLoad(dll.FileName!, out var handle))
            {
                throw new InvalidOperationException();
            }

            if (!NativeLibrary.TryGetExport(handle, "ServiceMain", out var address))
            {
                throw new Exception($"动态链接库【{dll.FileName}】中没有导出 ServiceMain");
            }


            List<nint> nintArgs = new();
            if (dll.Args != null && dll.Args.Length > 0)
            {
                foreach (var arg in dll.Args)
                {
                    nintArgs.Add(Marshal.StringToHGlobalAnsi(arg));
                }
            }
            List.TryAdd(address, nintArgs);
            return address;
        }

        /// <summary>
        /// 函数地址  args
        /// </summary>
        public static ConcurrentDictionary<nint, List<nint>> List { get; set; } = new();
    }
}
