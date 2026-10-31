using UsmpConnect.Services.Agente;

namespace UsmpConnect.Tests;

public class HorarioRestauranteTests
{
    [Theory]
    [InlineData("Lun - Dom: 5:00 PM - 1:00 AM", DayOfWeek.Friday, false)]
    [InlineData("Lun - Dom: 12:00 PM - 11:00 PM", DayOfWeek.Friday, true)]
    [InlineData("Lun - Vie: 8:00 AM - 9:00 PM · Sáb: 8:30 AM - 1:00 PM", DayOfWeek.Saturday, true)]
    [InlineData("Lun - Vie: 8:00 AM - 9:00 PM · Sáb: 8:30 AM - 1:00 PM", DayOfWeek.Sunday, false)]
    [InlineData("Consultar en el local", DayOfWeek.Friday, null)]
    [InlineData("Lun - Dom: 8:00 AM - 10:00 AM · Lun - Dom: 12:00 PM - 4:00 PM", DayOfWeek.Friday, true)]
    public void Almuerzo_UsaHorarioPublicadoSinPrometerAperturaReal(string horario, DayOfWeek dia, bool? esperado)
        => Assert.Equal(esperado, HorarioRestaurante.IncluyeAlmuerzo(horario, dia));

    [Fact]
    public void Seguimiento_CambiaCategoriaYConservaPresupuesto()
    {
        var consulta = ConsultaAsistente.Crear("y chifa", [new("usuario", "Dónde almorzar por 15 soles, comida rápida"), new("usuario", "por 20?")]);
        Assert.Equal(20m, consulta.Presupuesto);
        Assert.Equal("chifa", consulta.Categoria);
        Assert.True(consulta.ParaAlmorzar);
        var siguiente = ConsultaAsistente.Crear("por 18", [new("usuario", "Dónde almorzar por 15 soles, comida rápida"), new("usuario", "por 20?"), new("usuario", "y chifa")]);
        Assert.Equal(18m, siguiente.Presupuesto);
        Assert.Equal("chifa", siguiente.Categoria);
        Assert.True(siguiente.ParaAlmorzar);
        Assert.Equal("apuntes", ConsultaAsistente.Crear("ahora apuntes de cálculo", [new("usuario", consulta.Contexto)]).Tema);
    }
}
