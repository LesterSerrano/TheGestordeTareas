using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;
// using GestordeTareas.;

namespace GestordeTareas.DAL.Interfaces
{
     public interface IPrioridad
    {
        Task<Prioridad> GetPrioridadByIdAsync(int id);
        Task<IEnumerable<Prioridad>> GetAllPrioridadesAsync();
        Task<Prioridad> CreatePrioridadAsync(Prioridad prioridad);
        Task<Prioridad> UpdatePrioridadAsync(Prioridad prioridad);
        Task<bool> DeletePrioridadAsync(int id);
        
        // Si quisieras paginación, podrías agregar:
    }
}