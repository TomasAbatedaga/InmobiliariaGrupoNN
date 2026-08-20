using InmobiliariaGrupoNN.Models;
using System.Collections.Generic;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioPropietario
    {
        IList<Propietario> ObtenerTodos();
        Propietario? ObtenerPorId(int id);
        int Alta(Propietario propietario);
        int Modificacion(Propietario propietario);
        int Baja(int id);
    }
}