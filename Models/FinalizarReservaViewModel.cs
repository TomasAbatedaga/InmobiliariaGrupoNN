namespace InmobiliariaGrupoNN.Models;

public class FinalizarReservaViewModel
{
    public Reserva Reserva { get; set; } = new();
    public DateTime FechaFinalizacion { get; set; }
    public int DuracionOriginal { get; set; }
    public int DiasTranscurridos { get; set; }
    public int DiasRestantes { get; set; }
    public decimal PorcentajeMulta { get; set; }
    public decimal ImporteMulta { get; set; }
    public bool CalculoValido { get; set; }
}
