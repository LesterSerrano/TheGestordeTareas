using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;

namespace GestordeTareas.DAL.Interfaces
{
    public interface IProyecto
    {
        Task<IEnumerable<Proyecto>> GetAllProyectAsync();
        Task<Proyecto> GetByIdAsync(int id);
        Task <Proyecto> CreateAsync(Proyecto proyecto);
        Task<bool> DeleteAsync(Proyecto proyecto);
        Task<Proyecto> UpdateAsync(Proyecto proyecto);
        //el resto de los metodos
        Task<string> GenerarCodigoAcceso();
        Task<bool> ExisteCodigoAcceso(string codigoAcceso);
        Task<IEnumerable<Proyecto>> BuscarPorTituloOAdministradorAsync(string query);
    }
}