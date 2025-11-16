using GestordeTaras.EN;
using GestordeTareas.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestordeTareas.BL
{
    public class PrioridadBL
    {
        private readonly IPrioridad _prioridadDAL;

        public PrioridadBL(IPrioridad prioridadDAL)
        {
            _prioridadDAL = prioridadDAL;
        }

        public async Task<Prioridad> CreateAsync(Prioridad prioridad)
        {
            return await _prioridadDAL.CreatePrioridadAsync(prioridad);
        }

        public async Task<Prioridad> UpdateAsync(Prioridad prioridad)
        {
            return await _prioridadDAL.UpdatePrioridadAsync(prioridad);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _prioridadDAL.DeletePrioridadAsync(id);
        }

        public async Task<Prioridad> GetByIdAsync(int id)
        {
            return await _prioridadDAL.GetPrioridadByIdAsync(id);
        }

        public async Task<IEnumerable<Prioridad>> GetAllAsync()
        {
            return await _prioridadDAL.GetAllPrioridadesAsync();
        }
    }
}
