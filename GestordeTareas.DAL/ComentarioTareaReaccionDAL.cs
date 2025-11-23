using System;
using System.Threading.Tasks;
using GestordeTaras.EN;
using GestordeTareas.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestordeTareas.DAL
{
    public class ComentarioTareaReaccionDAL : IComentarioTareaReaccion
    {
        private readonly ContextoBD _context;
        private readonly ILogger<ComentarioTareaReaccionDAL> _logger;

        public ComentarioTareaReaccionDAL(ContextoBD contextoBD, ILogger<ComentarioTareaReaccionDAL> logger)
        {
            _context = contextoBD;
            _logger = logger;
        }

        /// <summary>
        /// Crear o registrar una reacción en un comentario.
        /// Retorna el Id generado de la reacción.
        /// </summary>
        public async Task<int> CrearReaccionAsync(ComentarioTareaReaccion reaccion)
        {
            if (reaccion == null)
                throw new ArgumentNullException(nameof(reaccion), "El objeto reacción no puede ser nulo.");

            if (reaccion.IdComentario <= 0)
                throw new ArgumentException("El ID del comentario es inválido.", nameof(reaccion.IdComentario));

            if (reaccion.IdUsuario <= 0)
                throw new ArgumentException("El ID del usuario es inválido.", nameof(reaccion.IdUsuario));

            if (reaccion.TipoReaccion < 1)
                throw new ArgumentException("El tipo de reacción no es válido.", nameof(reaccion.TipoReaccion));

            try
            {
                // Validar que el comentario exista
                var comentarioExistente = await _context.ComentarioTarea
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == reaccion.IdComentario);

                if (comentarioExistente == null)
                {
                    _logger.LogWarning("Intento de crear reacción para comentario inexistente: {IdComentario}", reaccion.IdComentario);
                    throw new Exception("El comentario no existe.");
                }

                // Fecha actual
                reaccion.Fecha = DateTime.UtcNow;

                await _context.ComentarioTareaReaccion.AddAsync(reaccion);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reacción creada correctamente con ID {IdReaccion} para el comentario {IdComentario}.", reaccion.Id, reaccion.IdComentario);

                return reaccion.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reacción para comentario {IdComentario}.", reaccion?.IdComentario);
                throw;
            }
        }
        /// <summary>
        /// Actualiza el tipo de reacción de un comentario existente.
        /// Retorna true si la operación fue exitosa.
        /// </summary>
        public async Task<bool> ActualizarReaccionAsync(ComentarioTareaReaccion reaccion)
        {
            if (reaccion == null)
                throw new ArgumentNullException(nameof(reaccion), "El objeto reacción no puede ser nulo.");

            if (reaccion.Id <= 0)
                throw new ArgumentException("El ID de la reacción es inválido.", nameof(reaccion.Id));

            if (reaccion.TipoReaccion < 1)
                throw new ArgumentException("El tipo de reacción no es válido.", nameof(reaccion.TipoReaccion));

            try
            {
                // Buscar la reacción existente
                var reaccionExistente = await _context.ComentarioTareaReaccion
                    .FirstOrDefaultAsync(r => r.Id == reaccion.Id);

                if (reaccionExistente == null)
                {
                    _logger.LogWarning("Intento de actualizar una reacción inexistente: {Id}", reaccion.Id);
                    return false;
                }

                // Actualizar solo el tipo de reacción y la fecha
                reaccionExistente.TipoReaccion = reaccion.TipoReaccion;
                reaccionExistente.Fecha = DateTime.UtcNow;

                _context.ComentarioTareaReaccion.Update(reaccionExistente);
                var cambios = await _context.SaveChangesAsync();

                _logger.LogInformation("Reacción {Id} actualizada correctamente a tipo {Tipo}.", reaccion.Id, reaccion.TipoReaccion);
                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reacción con ID {Id}.", reaccion?.Id);
                throw;
            }
        }
        /// <summary>
        /// Elimina una reacción de un comentario usando su ID.
        /// Retorna true si la operación fue exitosa.
        /// </summary>
        public async Task<bool> EliminarReaccionAsync(int idReaccion)
        {
            if (idReaccion <= 0)
                throw new ArgumentException("El ID de la reacción no es válido.", nameof(idReaccion));

            try
            {
                // Buscar la reacción en la base de datos
                var reaccion = await _context.ComentarioTareaReaccion
                    .FirstOrDefaultAsync(r => r.Id == idReaccion);

                if (reaccion == null)
                {
                    _logger.LogWarning("Intento de eliminar una reacción inexistente: {Id}", idReaccion);
                    return false;
                }

                // Eliminar la reacción
                _context.ComentarioTareaReaccion.Remove(reaccion);
                var cambios = await _context.SaveChangesAsync();

                _logger.LogInformation("Reacción {Id} eliminada correctamente.", idReaccion);
                return cambios > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la reacción con ID {Id}.", idReaccion);
                throw;
            }
        }
        // public async Task<bool> ToggleReaccionAsync(int idComentario, int idUsuario, byte tipoReaccion)
        // {
        //     // Validaciones
        //     if (idComentario <= 0) throw new ArgumentException("ID de comentario inválido.", nameof(idComentario));
        //     if (idUsuario <= 0) throw new ArgumentException("ID de usuario inválido.", nameof(idUsuario));

        //     // Buscar si el usuario ya reaccionó a este comentario
        //     var reaccionExistente = await _reaccionDal.ObtenerReaccionUsuarioAsync(idComentario, idUsuario);

        //     if (reaccionExistente != null)
        //     {
        //         // Si existe, eliminarla (toggle OFF)
        //         return await _reaccionDal.EliminarReaccionAsync(reaccionExistente.Id);
        //     }
        //     else
        //     {
        //         // Si no existe, crear la reacción (toggle ON)
        //         var nuevaReaccion = new ComentarioTareaReaccion
        //         {
        //             IdComentario = idComentario,
        //             IdUsuario = idUsuario,
        //             TipoReaccion = tipoReaccion,
        //             Fecha = DateTime.Now
        //         };
        //         return await _reaccionDal.CrearReaccionAsync(nuevaReaccion) > 0;
        //     }
        // }
        /// <summary>
        /// Obtiene la reacción de un usuario sobre un comentario específico.
        /// Retorna null si no existe.
        /// </summary>
        public async Task<ComentarioTareaReaccion> ObtenerReaccionUsuarioAsync(int idComentario, int idUsuario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario no es válido.", nameof(idUsuario));

            try
            {
                var reaccion = await _context.ComentarioTareaReaccion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.IdComentario == idComentario && r.IdUsuario == idUsuario);

                return reaccion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la reacción del usuario {UserId} para el comentario {CommentId}.", idUsuario, idComentario);
                throw;
            }
        }
        /// <summary>
        /// Obtiene todas las reacciones asociadas a un comentario.
        /// Retorna una colección vacía si no hay reacciones.
        /// </summary>
        public async Task<IEnumerable<ComentarioTareaReaccion>> ObtenerReaccionesPorComentarioAsync(int idComentario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            try
            {
                var reacciones = await _context.ComentarioTareaReaccion
                    .AsNoTracking()
                    .Where(r => r.IdComentario == idComentario)
                    .OrderByDescending(r => r.Fecha) // opcional: mostrar las más recientes primero
                    .ToListAsync();

                return reacciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reacciones para el comentario {CommentId}.", idComentario);
                throw;
            }
        }
        /// <summary>
        /// Cuenta el total de reacciones asociadas a un comentario.
        /// Retorna 0 si no existen reacciones.
        /// </summary>
        public async Task<int> ContarReaccionesAsync(int idComentario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            try
            {
                var total = await _context.ComentarioTareaReaccion
                    .AsNoTracking()
                    .CountAsync(r => r.IdComentario == idComentario);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar las reacciones del comentario {CommentId}.", idComentario);
                throw;
            }
        }
        /// <summary>
        /// Obtiene un resumen de reacciones de un comentario, agrupadas por tipo.
        /// Retorna un diccionario donde la clave es el tipo de reacción y el valor es la cantidad.
        /// </summary>
        public async Task<Dictionary<byte, int>> ObtenerResumenReaccionesAsync(int idComentario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            try
            {
                var resumen = await _context.ComentarioTareaReaccion
                    .AsNoTracking()
                    .Where(r => r.IdComentario == idComentario)
                    .GroupBy(r => r.TipoReaccion)
                    .Select(g => new { Tipo = g.Key, Count = g.Count() })
                    .ToListAsync();

                return resumen.ToDictionary(r => r.Tipo, r => r.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el resumen de reacciones del comentario {CommentId}.", idComentario);
                throw;
            }
        }


    }
}
