using System.ComponentModel;
using Microsoft.SemanticKernel;
using UsmpConnect.Services.Conocimiento;

namespace UsmpConnect.Services.Agente;

public sealed class ConocimientoFiaPlugin(IKnowledgeBaseService conocimiento, ContextoAsistente contexto)
{
    [KernelFunction, Description("Busca fuentes sobre el funcionamiento de USMP Connect y documentos de referencia de la facultad. No inventa normativa; devuelve el modo de búsqueda y sus fuentes.")]
    public async Task<object> BuscarEnConocimiento(string consulta, CancellationToken cancellationToken = default)
    {
        consulta = consulta.Trim();
        if (consulta.Length is < 3 or > 500) return new { error = "Consulta de entre 3 y 500 caracteres." };
        var resultado = await conocimiento.BuscarAsync(consulta, 5, cancellationToken);
        foreach (var fuente in resultado.Fuentes)
            if (contexto.Fuentes.Count < 5 && !contexto.Fuentes.Contains(fuente)) contexto.Fuentes.Add(fuente);
        return resultado;
    }
}
