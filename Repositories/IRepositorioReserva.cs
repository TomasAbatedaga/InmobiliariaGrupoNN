using System.Collections.Generic;
using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioReserva
    {
        IList<Reserva> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);
        Reserva? ObtenerPorId(int id);
        int Alta(Reserva reserva);
        int Modificacion(Reserva reserva);
        int Baja(int id);
    }
}