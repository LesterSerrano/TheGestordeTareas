using GestordeTaras.EN;
using GestordeTareas.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestordeTareas.DAL
{
    public class PrioridadDAL : IPrioridad
    {
        //  Task<Prioridad> GetPrioridadByIdAsync(int id);
        // Task<IEnumerable<Prioridad>> GetAllPrioridadesAsync();
        // Task<int> CreatePrioridadAsync(Prioridad prioridad);
        // Task<int> UpdatePrioridadAsync(Prioridad prioridad);
        // Task<bool> DeletePrioridadAsync(int id);
        private readonly ContextoBD _dbContext;
        private readonly ILogger<PrioridadDAL> _logger;

        public PrioridadDAL(ContextoBD dbContext, ILogger<PrioridadDAL> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        //-------------------------------- CREAR PRIORIDAD --------------------------
        public async Task<Prioridad> CreatePrioridadAsync(Prioridad prioridad)
        {
            var existePrioridad = await _dbContext.Prioridad
                .AsNoTracking()
                .AnyAsync(p => p.Nombre.ToLower() == prioridad.Nombre.ToLower());
            if (existePrioridad)
            {
                _logger.LogWarning($"Ya existe una prioridad con el nombre '{prioridad.Nombre}'.");
                return new Prioridad { Id = 0, Nombre = string.Empty };
            }
            await _dbContext.Prioridad.AddAsync(prioridad);
            await _dbContext.SaveChangesAsync();
            return prioridad;
        }

        //-------------------------------- MODIFICAR PRIORIDAD --------------------------
        public async Task<Prioridad> UpdatePrioridadAsync(Prioridad prioridad)
        {
            var prioridadBD = await GetPrioridadByIdAsync(prioridad.Id);

            var nombreExiste = await _dbContext.Prioridad
                .AsNoTracking()
                .AnyAsync(p => p.Nombre.ToLower() == prioridad.Nombre.ToLower() && p.Id != prioridad.Id);
            if (nombreExiste)
            {
                _logger.LogWarning($"No se puede actualizar. El nombre '{prioridad.Nombre}' ya está en uso por otra prioridad.");
                return new Prioridad { Id = 0, Nombre = "NOMBRE_DUPLICADO" };
            }

            prioridadBD.Nombre = prioridad.Nombre;

            _dbContext.Prioridad.Update(prioridadBD);
            await _dbContext.SaveChangesAsync();

            return prioridadBD;
        }

        //-------------------------------- ELIMINAR PRIORIDAD --------------------------
        public async Task<bool> DeletePrioridadAsync(int id)
        {
            var prioridadBD = await GetPrioridadByIdAsync(id);

            if (prioridadBD.Id == 0)
            {
                _logger.LogWarning($"No se puede eliminar. La prioridad con ID {id} no existe.");
                return false;
            }

            // Verificar si hay tareas asociadas a esta prioridad
            bool tieneTareasAsociadas = await _dbContext.Tarea
                .AsNoTracking()
                .AnyAsync(t => t.IdPrioridad == prioridadBD.Id);

            if (tieneTareasAsociadas)
            {
                _logger.LogWarning($"No se puede eliminar la prioridad con ID {id} porque tiene tareas asociadas.");
                return false;
            }

            _dbContext.Prioridad.Remove(prioridadBD);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        //-------------------------- BUSCAR POR ID --------------------------
        public async Task<Prioridad> GetPrioridadByIdAsync(int id)
        {
            var prioridad = await _dbContext.Prioridad
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (prioridad == null)
            {
                _logger.LogWarning($"Prioridad con ID {id} no encontrado.");
                return new Prioridad
                {
                    Id = 0,
                    Nombre = string.Empty
                };
            }
            return prioridad;
        }

        //-------------------------------- LISTAR TODAS --------------------------
        public async Task<IEnumerable<Prioridad>> GetAllPrioridadesAsync()
        {
            var prioridades = await _dbContext.Prioridad
                .AsNoTracking()
                .ToListAsync();
            if (prioridades.Count == 0)
            {
                _logger.LogWarning("No se encontraron prioridades.");
            }
            return prioridades;
        }
    }
}
