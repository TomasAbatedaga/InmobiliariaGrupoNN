using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioInmueble
    {
        IList<Inmueble> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);

        IList<Inmueble> BuscarDisponibles(DateTime fechaInicio, DateTime fechaFin,
            int? cupoMinimo, string? tipo, int numeroPagina = 1, int tamanio = 10);

        int ObtenerCantidadDisponibles(DateTime fechaInicio, DateTime fechaFin,
            int? cupoMinimo, string? tipo);
        Inmueble? ObtenerPorId(int id);

        int Alta(Inmueble inmueble);

        int Modificacion(Inmueble inmueble);

        int Baja(int id);

        int ModificarPortada(int id, string? ruta);
    }
}