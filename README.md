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

    RunMain(args.ToArray());
}

[UnmanagedCallersOnly(EntryPoint = "ServiceStop", CallConvs = [typeof(CallConvCdecl)])]
public static void ServiceStop()
{
  if (host != null)
  {
    host.StopAsync();
  }
}
```
