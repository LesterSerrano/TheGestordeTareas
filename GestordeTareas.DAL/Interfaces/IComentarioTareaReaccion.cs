using System.Collections.Generic;
using System.Threading.Tasks;
using GestordeTaras.EN;

namespace GestordeTareas.DAL.Interfaces
{
    public interface IComentarioTareaReaccion
    {
        // Crear o registrar una reacción en un comentario
        Task<int> CrearReaccionAsync(ComentarioTareaReaccion reaccion);

        // Actualizar el tipo de reacción (ej: Like → Love)
        Task<bool> ActualizarReaccionAsync(ComentarioTareaReaccion reaccion);

        // Eliminar una reacción por su ID
        Task<bool> EliminarReaccionAsync(int idReaccion);

        // Obtener la reacción de un usuario específico sobre un comentario
        Task<ComentarioTareaReaccion> ObtenerReaccionUsuarioAsync(int idComentario, int idUsuario);

        // Obtener todas las reacciones de un comentario
        Task<IEnumerable<ComentarioTareaReaccion>> ObtenerReaccionesPorComentarioAsync(int idComentario);

        // Contar el total de reacciones de un comentario
        Task<int> ContarReaccionesAsync(int idComentario);

        // Obtener un resumen de reacciones por tipo (like, love, etc.)
        Task<Dictionary<byte, int>> ObtenerResumenReaccionesAsync(int idComentario);
    }
}
