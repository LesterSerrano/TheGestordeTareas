using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;
using Microsoft.Extensions.Logging;

namespace GestordeTareas.DAL.Utils
{
    public class ValidateComentarioTarea
    {

        public static void Validar(ComentarioTarea comentario, ILogger logger)
        {
            if (comentario == null)
            {
                logger.LogError("El comentario recibido es nulo.");
                throw new ArgumentNullException(nameof(comentario));
            }

            if (string.IsNullOrWhiteSpace(comentario.Comentario))
            {
                logger.LogWarning("Comentario vacío detectado.");
                throw new ArgumentException("El texto del comentario no puede estar vacío.");
            }

            // Normalizar
            comentario.Comentario = comentario.Comentario.Trim();

            // Estado por defecto
            if (comentario.Estado == 0)
                comentario.Estado = 1;

            // Validar ID de padre
            if (comentario.IdComentarioPadre.HasValue && comentario.IdComentarioPadre <= 0)
            {
                logger.LogWarning("IdComentarioPadre inválido, se limpiará.");
                comentario.IdComentarioPadre = null;
            }
        }
    }
}