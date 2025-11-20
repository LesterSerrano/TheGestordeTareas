using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;
using GestordeTareas.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestordeTareas.DAL
{
    public class ComentarioTareaDAL : IComentarioTarea
    {
        private readonly ContextoBD _contextoBD;
        private readonly ILogger<ComentarioTareaDAL> _logger;
        public ComentarioTareaDAL(ILogger<ComentarioTareaDAL> logger, ContextoBD contextoBD)
        {
            _logger = logger;
            _contextoBD = contextoBD;
        }
        public async Task<ComentarioTarea> CrearComentarioAsync(ComentarioTarea comentarioTarea)
        {
            try
            {
                // Vaqui van todas las vALIDACIONES -------------------------------
                if (comentarioTarea == null)
                {
                    _logger.LogError("El comentario recibido es nulo.");
                    throw new ArgumentNullException(nameof(comentarioTarea));
                }

                if (string.IsNullOrWhiteSpace(comentarioTarea.Comentario))
                {
                    _logger.LogWarning("Comentario vacío detectado.");
                    throw new ArgumentException("El texto del comentario no puede estar vacío.");
                }

                // -------------- NORMALIZAR EL TEXTO ------------------------
                comentarioTarea.Comentario = comentarioTarea.Comentario.Trim();

                comentarioTarea.FechaCreacion = DateTime.UtcNow;
                // Validar IdComentarioPadre:
                // Si el comentario viene marcado como respuesta (tiene un IdComentarioPadre),
                // pero el valor es menor o igual a 0, significa que es un ID inválido.
                // Los Ids en la base de datos siempre son positivos, por lo que valores como 0 o negativos
                // no representan un comentario existente. En ese caso, se registra una advertencia
                // y se limpia el campo para tratarlo como un comentario raíz (sin padre).
                if (comentarioTarea.IdComentarioPadre.HasValue && comentarioTarea.IdComentarioPadre <= 0)
                {
                    _logger.LogWarning("IdComentarioPadre es inválido.");
                    comentarioTarea.IdComentarioPadre = null;
                }
                if (comentarioTarea.Estado == 0)
                    comentarioTarea.Estado = 1;
                if (comentarioTarea.IdComentarioPadre.HasValue)
                {
                    var existePadre = await _contextoBD.ComentarioTarea
                        .AnyAsync(c => c.Id == comentarioTarea.IdComentarioPadre);

                    if (!existePadre)
                    {
                        _logger.LogWarning("El comentario padre con ID {Id} no existe.", comentarioTarea.IdComentarioPadre);
                        throw new ArgumentException("El comentario padre no existe.");
                    }
                }


                await _contextoBD.ComentarioTarea.AddAsync(comentarioTarea);
                await _contextoBD.SaveChangesAsync();

                _logger.LogInformation("Comentario creado correctamente con ID {Id}.", comentarioTarea.Id);

                return comentarioTarea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el comentario.");
                throw;
            }
        }


    }
}