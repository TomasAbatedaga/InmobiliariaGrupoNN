using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioPago
    {
        IList<Pago> ObtenerPorReserva(
            int reservaId,
            int numeroPagina = 1,
            int tamanio = 10);

        int ObtenerCantidadPorReserva(int reservaId);

        Pago? ObtenerPorId(int id);

        int Alta(Pago pago);

        int ModificarConcepto(int id, string concepto);

        int Anular(int id);
    }
}