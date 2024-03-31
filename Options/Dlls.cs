using System.Runtime.InteropServices;

namespace NetCoreSvchost.Options
{
    public class DllList
    {
        public required Dll[] Dlls { get; set; }
    }

    public class Dll
    {
        public required string FileName { get; set; }
        public string[]? Args { get; set; }

        public static DllDetail GetDllDetail(Dll dll)
        {
            if (!NativeLibrary.TryLoad(dll.FileName!, out var handle))
            {
                throw new InvalidOperationException();
            }

            if (!NativeLibrary.TryGetExport(handle, "ServiceMain", out var serviceMainFuncAddress))
            {
                throw new Exception($"动态链接库【{dll.FileName}】中没有导出 ServiceMain");
            }

            if (!NativeLibrary.TryGetExport(handle, "ServiceStop", out var serviceStopFuncAddress))
            {
                throw new Exception($"动态链接库【{dll.FileName}】中没有导出 ServiceStop");
            }

            List<nint> nintArgs = new();
            if (dll.Args != null && dll.Args.Length > 0)
            {
                foreach (var arg in dll.Args)
                {
                    nintArgs.Add(Marshal.StringToHGlobalAnsi(arg));
                }
            }

            return new DllDetail(dll.FileName, serviceMainFuncAddress, serviceStopFuncAddress, nintArgs);
        }
    }

    public class DllDetail
    {
        public unsafe delegate* unmanaged[Cdecl]<int, nint[], void> ServiceMain { get; }
        public unsafe delegate* unmanaged[Cdecl]<void> ServiceStop { get; }

        public string FileName { get; }

        public nint[] Args { get; }

        public unsafe DllDetail(string fileName, nint serviceMainFuncAddress, nint serviceStopFuncAddress, List<nint> nintArgs)
        {
            this.FileName = fileName;
            this.ServiceMain = (delegate* unmanaged[Cdecl]<int, nint[], void>)serviceMainFuncAddress;
            this.ServiceStop = (delegate* unmanaged[Cdecl]<void>)serviceStopFuncAddress;
            this.Args = nintArgs.ToArray();
        }
    }
}
