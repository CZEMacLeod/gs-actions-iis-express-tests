using System.Diagnostics;
using Xunit.Abstractions;

namespace C3D.IISTestapp.Tests;

public class IISExpressFixture
{
    internal HttpClient CreateClient(int port)
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
        };
        var client = new HttpClient(handler);
        client.BaseAddress = new UriBuilder("http", "localhost", port).Uri;
        return client;
    }



    internal async Task<IISExpressProcess> StartAsync(int port, ITestOutputHelper outputHelper, params IEnumerable<(string name, string value)> env)
    {
        var path = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "src", "C3D.IISOwinApp")).FullName;

        var psi = new ProcessStartInfo()
        {
            FileName = "C:\\Program Files\\IIS Express\\iisexpress.exe",
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add($"/path:{path}");
        psi.ArgumentList.Add($"/port:{port}");
        psi.ArgumentList.Add("/clr:v4.0");
        psi.ArgumentList.Add("/systray:false");
        foreach (var e in env)
        {
            psi.EnvironmentVariables[e.name]= e.value;
        }

        var process = new Process()
        {
            StartInfo = psi,
            EnableRaisingEvents = true,
        };
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputHelper.WriteLine(e.Data);
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputHelper.WriteLine(e.Data);
            }
        };

        process.Start();

        return new IISExpressProcess(process,port);
    }
}