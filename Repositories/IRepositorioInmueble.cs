using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioInmueble
    {
        IList<Inmueble> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);

        Inmueble? ObtenerPorId(int id);

        int Alta(Inmueble inmueble);

        int Modificacion(Inmueble inmueble);

        int Baja(int id);

        int ModificarPortada(int id, string? ruta);
    }
}