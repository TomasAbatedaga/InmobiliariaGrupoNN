using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioImagen
    {
        int Alta(Imagen imagen);

        int Baja(int id);

        int Modificacion(Imagen imagen);

        Imagen? ObtenerPorId(int id);

        IList<Imagen> BuscarPorInmueble(int inmuebleId);
    }
}