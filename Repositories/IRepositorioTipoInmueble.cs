using System.Collections.Generic;
using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioTipoInmueble
    {
        IList<TipoInmueble> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);
        int ObtenerTotal();
        TipoInmueble? ObtenerPorId(int id);
        int Alta(TipoInmueble tipo);
        int Modificacion(TipoInmueble tipo);
        int Baja(int id);
    }
}