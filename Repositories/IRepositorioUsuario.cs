using System.Collections.Generic;
using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public interface IRepositorioUsuario
    {
        IList<Usuario> ObtenerTodos(int numeroPagina = 1, int tamanio = 10);
        
        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorEmail(string email); 
        int Alta(Usuario usuario);
        int Modificacion(Usuario usuario);
        int Baja(int id);
    }
}