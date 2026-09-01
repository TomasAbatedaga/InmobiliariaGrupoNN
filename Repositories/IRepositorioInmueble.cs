using InmobiliariaGrupoNN.Models;
using System.Collections.Generic;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioInmueble
    {
        IList<Inmueble> ObtenerTodos();

        Inmueble? ObtenerPorId(int id);

        int Alta(Inmueble inmueble);

        int Modificacion(Inmueble inmueble);

        int Baja(int id);
    }
}