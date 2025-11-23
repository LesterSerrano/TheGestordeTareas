using System.Collections.Generic;
using System.Threading.Tasks;
using GestordeTaras.EN;

namespace GestordeTareas.BL.Interfaces
{
    public interface IComentarioTareaReaccionBL
    {
        // Registrar una reacción nueva (Like, Love, etc.)
        Task<int> CrearReaccionAsync(ComentarioTareaReaccion reaccion, int idUsuarioActual);

        // Cambiar el tipo de reacción de un usuario (ej: Like → Love)
        Task<bool> ActualizarReaccionAsync(ComentarioTareaReaccion reaccion, int idUsuarioActual);

        // Eliminar una reacción (solo si es del usuario actual)
        Task<bool> EliminarReaccionAsync(int idReaccion, int idUsuarioActual);

        // Obtener la reacción que hizo un usuario a un comentario
        Task<ComentarioTareaReaccion> ObtenerReaccionUsuarioAsync(int idComentario, int idUsuarioActual);

        // Obtener todas las reacciones de un comentario
        Task<IEnumerable<ComentarioTareaReaccion>> ObtenerReaccionesPorComentarioAsync(int idComentario);

        // Contar reacciones de un comentario
        Task<int> ContarReaccionesAsync(int idComentario);

        // Resumen agrupado por tipo de reacción
        Task<Dictionary<byte, int>> ObtenerResumenReaccionesAsync(int idComentario);
    }
}
