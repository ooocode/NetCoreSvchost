## 内部使用，请勿下载

### C#实现Dll,导出两个函数
分别是主服务入口、停止

```csharp
[UnmanagedCallersOnly(EntryPoint = "ServiceMain", CallConvs = [typeof(CallConvCdecl)])]
public static unsafe void ServiceMain(int argc, nint* argv)
{
    List<string> args = new List<string>();
    for (int i = 0; i < argc; i++)
    {
        var arg = Marshal.PtrToStringAnsi(argv[i]);
        if (!string.IsNullOrWhiteSpace(arg))
        {
            args.Add(arg);
        }
    }
}

[UnmanagedCallersOnly(EntryPoint = "ServiceStop", CallConvs = [typeof(CallConvCdecl)])]
public static void ServiceStop()
{

}
```
