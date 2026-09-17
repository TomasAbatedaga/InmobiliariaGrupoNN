using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioInquilino
    {
        IList<Inquilino> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);
        int ObtenerTotal();

        Inquilino? ObtenerPorId(int id);

        int Alta(Inquilino inquilino);

        int Modificacion(Inquilino inquilino);

        int Baja(int id);
    }
}