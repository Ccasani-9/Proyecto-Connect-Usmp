using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace UsmpConnect.Tests;

/// <summary>Interrumpe solo las conexiones de una prueba, sin apagar el Redis compartido.</summary>
internal sealed class RedisTestProxy : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource stopping = new();
    private readonly ConcurrentBag<TcpClient> clients = [];
    private readonly string targetHost;
    private readonly int targetPort;
    private readonly Task accepting;
    private volatile bool enabled = true;

    public RedisTestProxy(string host, int port)
    {
        targetHost = host;
        targetPort = port;
        listener.Start();
        Endpoint = (IPEndPoint)listener.LocalEndpoint;
        accepting = AcceptAsync();
    }

    public IPEndPoint Endpoint { get; }

    public void Disconnect()
    {
        enabled = false;
        foreach (var client in clients) client.Dispose();
    }

    public void Reconnect() => enabled = true;

    private async Task AcceptAsync()
    {
        try
        {
            while (!stopping.IsCancellationRequested)
            {
                var front = await listener.AcceptTcpClientAsync(stopping.Token);
                clients.Add(front);
                _ = ForwardAsync(front);
            }
        }
        catch (OperationCanceledException) { }
        catch (SocketException) when (stopping.IsCancellationRequested) { }
    }

    private async Task ForwardAsync(TcpClient front)
    {
        using (front)
        using (var back = new TcpClient())
        {
            clients.Add(back);
            try
            {
                if (!enabled) return;
                await back.ConnectAsync(targetHost, targetPort, stopping.Token);
                if (!enabled) return;
                var upstream = front.GetStream().CopyToAsync(back.GetStream(), stopping.Token);
                var downstream = back.GetStream().CopyToAsync(front.GetStream(), stopping.Token);
                await Task.WhenAny(upstream, downstream);
                front.Dispose();
                back.Dispose();
                await Task.WhenAll(upstream, downstream);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or OperationCanceledException) { }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await stopping.CancelAsync();
        listener.Stop();
        foreach (var client in clients) client.Dispose();
        await accepting;
        stopping.Dispose();
    }
}
