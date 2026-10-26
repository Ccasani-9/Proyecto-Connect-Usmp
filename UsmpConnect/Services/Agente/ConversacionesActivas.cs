using System.Collections.Concurrent;

namespace UsmpConnect.Services.Agente;

public sealed class ConversacionesActivas
{
    private readonly ConcurrentDictionary<string, byte> _activas = new();
    public bool IntentarAbrir(string clave) => _activas.TryAdd(clave, 0);
    public void Cerrar(string clave) => _activas.TryRemove(clave, out _);
}
