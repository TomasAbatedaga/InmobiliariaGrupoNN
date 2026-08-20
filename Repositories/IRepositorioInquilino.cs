using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioInquilino
    {
        IList<Inquilino> ObtenerTodos();

        Inquilino? ObtenerPorId(int id);

        int Alta(Inquilino inquilino);

        int Modificacion(Inquilino inquilino);

        int Baja(int id);
    }
}