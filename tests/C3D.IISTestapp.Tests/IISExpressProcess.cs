using System.Diagnostics;

namespace C3D.IISTestapp.Tests;

public class IISExpressProcess : IDisposable
{
    public Process Process { get; }
    public int Port { get; }

    public IISExpressProcess(Process process, int port)
    {
        Process = process;
        Port = port;
    }

    public void Dispose()
    {
        if (Process != null && !Process.HasExited)
        {
            Process.Kill();
            Process.Dispose();
        }
    }
}