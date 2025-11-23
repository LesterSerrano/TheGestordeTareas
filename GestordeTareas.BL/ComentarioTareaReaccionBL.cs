using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;
using GestordeTareas.BL.Interfaces;
using GestordeTareas.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestordeTareas.BL
{
    public class ComentarioTareaReaccionBL : IComentarioTareaReaccionBL
    {
        private readonly IComentarioTareaReaccion _comentarioTareaReaccion;
        private readonly ILogger<ComentarioTareaReaccionBL> _logger;
        public ComentarioTareaReaccionBL(IComentarioTareaReaccion comentarioTareaReaccion, ILogger<ComentarioTareaReaccionBL> logger)
        {
            _comentarioTareaReaccion = comentarioTareaReaccion;
            _logger = logger;
        }
        /// <summary>
        /// Crea una reacción para un comentario.
        /// Si el usuario ya reaccionó:
        ///   - Si es el mismo tipo → se elimina (toggle).
        ///   - Si es otro tipo → se actualiza.
        /// Retorna el ID de la reacción creada o actualizada.
        /// </summary>
        public async Task<int> CrearReaccionAsync(ComentarioTareaReaccion reaccion, int idUsuarioActual)
        {
            if (reaccion == null)
                throw new ArgumentNullException(nameof(reaccion), "La reacción no puede ser nula.");

            if (reaccion.IdComentario <= 0)
                throw new ArgumentException("IdComentario inválido.", nameof(reaccion.IdComentario));

            if (idUsuarioActual <= 0)
                throw new ArgumentException("IdUsuario inválido.", nameof(idUsuarioActual));

            if (reaccion.TipoReaccion < 1)
                throw new ArgumentException("Tipo de reacción inválido.", nameof(reaccion.TipoReaccion));


            try
            {
                // Forzamos el usuario desde sesión → no confiamos en el que viene del cliente
                reaccion.IdUsuario = idUsuarioActual;

                // Revisar si el usuario YA reaccionó a este comentario
                var reaccionExistente =
                    await _comentarioTareaReaccion.ObtenerReaccionUsuarioAsync(reaccion.IdComentario, idUsuarioActual);

                // NO EXISTE → Crear la reacción
                if (reaccionExistente == null)
                {
                    reaccion.Fecha = DateTime.UtcNow;

                    int idGenerado = await _comentarioTareaReaccion.CrearReaccionAsync(reaccion);

                    _logger.LogInformation(
                        "Reacción creada (ID {IdReaccion}) por usuario {IdUsuario} en comentario {IdComentario}.",
                        idGenerado, idUsuarioActual, reaccion.IdComentario
                    );

                    return idGenerado;
                }

                // YA EXISTE → 1) mismo tipo → eliminar (toggle)
                if (reaccionExistente.TipoReaccion == reaccion.TipoReaccion)
                {
                    await _comentarioTareaReaccion.EliminarReaccionAsync(reaccionExistente.Id);

                    _logger.LogInformation(
                        "Reacción eliminada (toggle) del usuario {IdUsuario} en comentario {IdComentario}.",
                        idUsuarioActual, reaccion.IdComentario
                    );

                    return 0; // 0 indica que se eliminó
                }

                // YA EXISTE → 2) tipo distinto → actualizar
                reaccionExistente.TipoReaccion = reaccion.TipoReaccion;
                reaccionExistente.Fecha = DateTime.UtcNow;

                bool actualizado = await _comentarioTareaReaccion.ActualizarReaccionAsync(reaccionExistente);

                if (!actualizado)
                    throw new Exception("No se pudo actualizar la reacción.");

                _logger.LogInformation(
                    "Reacción actualizada (ID {IdReaccion}) del usuario {IdUsuario} en comentario {IdComentario}.",
                    reaccionExistente.Id, idUsuarioActual, reaccion.IdComentario
                );

                return reaccionExistente.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al crear/actualizar reacción para comentario {IdComentario} por usuario {IdUsuario}.",
                    reaccion?.IdComentario, idUsuarioActual
                );
                throw;
            }
        }

        public async Task<bool> ActualizarReaccionAsync(ComentarioTareaReaccion reaccion, int idUsuarioActual)
        {
            if (reaccion == null)
                throw new ArgumentNullException(nameof(reaccion));

            if (reaccion.Id <= 0)
                throw new ArgumentException("El ID de la reacción es inválido.", nameof(reaccion.Id));

            if (reaccion.TipoReaccion < 1)
                throw new ArgumentException("El tipo de reacción no es válido.", nameof(reaccion.TipoReaccion));

            if (idUsuarioActual <= 0)
                throw new ArgumentException("El usuario actual es inválido.");

            try
            {
                // Validar que la reacción exista y pertenezca al usuario
                var existente = await _comentarioTareaReaccion.ObtenerReaccionUsuarioAsync(reaccion.Id, idUsuarioActual);

                if (existente == null)
                {
                    _logger.LogWarning("Intento de actualizar reacción inexistente: {Id}", reaccion.Id);
                    return false;
                }

                if (existente.IdUsuario != idUsuarioActual)
                {
                    _logger.LogWarning("Usuario {UserId} intentó modificar reacción ajena {Id}",
                        idUsuarioActual, reaccion.Id);

                    throw new UnauthorizedAccessException("No puedes modificar una reacción que no es tuya.");
                }

                // Actualizar usando la DAL
                existente.TipoReaccion = reaccion.TipoReaccion;
                existente.Fecha = DateTime.UtcNow;

                return await _comentarioTareaReaccion.ActualizarReaccionAsync(existente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reacción {Id}.", reaccion?.Id);
                throw;
            }
        }
        public async Task<bool> EliminarReaccionAsync(int idReaccion, int idUsuarioActual)
        {
            if (idReaccion <= 0)
                throw new ArgumentException("El ID de la reacción no es válido.", nameof(idReaccion));

            if (idUsuarioActual <= 0)
                throw new ArgumentException("El usuario actual es inválido.", nameof(idUsuarioActual));

            try
            {
                // Obtener la reacción y validar que exista
                var reaccion = await _comentarioTareaReaccion.ObtenerReaccionUsuarioAsync(idReaccion, idUsuarioActual);

                if (reaccion == null)
                {
                    _logger.LogWarning("Intento de eliminar reacción inexistente: {Id}", idReaccion);
                    return false;
                }

                // Regla de negocio: solo el creador puede eliminar su reacción
                if (reaccion.IdUsuario != idUsuarioActual)
                {
                    _logger.LogWarning(
                        "Usuario {UserId} intentó eliminar una reacción que no es suya (Reacción {ReaId}).",
                        idUsuarioActual, idReaccion);

                    throw new UnauthorizedAccessException("No puedes eliminar una reacción que no te pertenece.");
                }

                // Delegar a la DAL para eliminar
                return await _comentarioTareaReaccion.EliminarReaccionAsync(idReaccion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la reacción con ID {Id}.", idReaccion);
                throw;
            }
        }
        public async Task<ComentarioTareaReaccion> ObtenerReaccionUsuarioAsync(int idComentario, int idUsuarioActual)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            if (idUsuarioActual <= 0)
                throw new ArgumentException("El ID del usuario actual no es válido.", nameof(idUsuarioActual));

            try
            {
                // Delegar completamente a la DAL
                var reaccion = await _comentarioTareaReaccion
                    .ObtenerReaccionUsuarioAsync(idComentario, idUsuarioActual);

                return reaccion; // Puede ser null y eso está bien
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener la reacción del usuario {UserId} para el comentario {CommentId}.",
                    idUsuarioActual,
                    idComentario
                );
                throw;
            }
        }
        public async Task<IEnumerable<ComentarioTareaReaccion>> ObtenerReaccionesPorComentarioAsync(int idComentario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            try
            {
                var reacciones = await _comentarioTareaReaccion
                    .ObtenerReaccionesPorComentarioAsync(idComentario);

                // La DAL ya retorna una lista vacía si no hay reacciones, lo cual es correcto
                return reacciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener las reacciones para el comentario {CommentId}.",
                    idComentario
                );
                throw;
            }
        }
        public async Task<int> ContarReaccionesAsync(int idComentario)
        {
            if (idComentario <= 0)
                throw new ArgumentException("El ID del comentario no es válido.", nameof(idComentario));

            try
            {
                var total = await _comentarioTareaReaccion
                    .ContarReaccionesAsync(idComentario);

                return total; // Si no hay reacciones, la DAL ya devuelve 0
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al contar las reacciones del comentario {CommentId}.",
                    idComentario
                );
                throw;
            }
        }
        /// <summary>
        /// Obtiene un resumen de reacciones agrupadas por tipo, con validación de negocio.
        /// </summary>
        public async Task<Dictionary<byte, int>> ObtenerResumenReaccionesAsync(int idComentario)
        {
            if (idComentario <= 0)
            {
                _logger.LogWarning("Se intentó obtener un resumen de reacciones con un ID inválido ({IdComentario}).", idComentario);
                throw new ArgumentException("El ID del comentario es inválido.");
            }

            try
            {
                var resumenDal = await _comentarioTareaReaccion.ObtenerResumenReaccionesAsync(idComentario);

                // Regla de negocio: si no hay reacciones, devolver diccionario vacío
                if (resumenDal == null || resumenDal.Count == 0)
                    return new Dictionary<byte, int>();

                return resumenDal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en BL al obtener resumen de reacciones para el comentario {CommentId}.", idComentario);
                throw; // la BL no debe tragarse errores críticos
            }

        }
    }
}
