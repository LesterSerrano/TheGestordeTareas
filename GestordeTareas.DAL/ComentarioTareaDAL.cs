using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestordeTaras.EN;
using GestordeTareas.DAL.Interfaces;
using GestordeTareas.DAL.Utils;
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
        public async Task<ComentarioTarea> CrearComentarioAsync(ComentarioTarea _coment)
        {
            try
            {
                // ---------- aqiui ocupo lo de los utilis  pero para vaidaciones generales netamente de la clase ----------
                ValidateComentarioTarea.Validar(_coment, _logger);

                // ---------- FECHA DE CREACIÓN ----------
                _coment.FechaCreacion = DateTime.UtcNow;

                // ---------- VALIDAR QUE EL PADRE EXISTA (si trae uno válido) ----------
                if (_coment.IdComentarioPadre.HasValue)
                {
                    bool existePadre = await _contextoBD.ComentarioTarea
                        .AnyAsync(c => c.Id == _coment.IdComentarioPadre);

                    if (!existePadre)
                    {
                        _logger.LogWarning("El comentario padre con ID {Id} no existe.", _coment.IdComentarioPadre);
                        throw new ArgumentException("El comentario padre no existe.");
                    }
                }

                // ---------- INSERTAR EN BASE DE DATOS ----------
                await _contextoBD.ComentarioTarea.AddAsync(_coment);
                await _contextoBD.SaveChangesAsync();

                _logger.LogInformation("Comentario creado correctamente con ID {Id}.", _coment.Id);

                return _coment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el comentario.");
                throw;
            }
        }
        public async Task<bool> EditarComentarioAsync(ComentarioTarea _coment)
        {
            try
            {
                ValidateComentarioTarea.Validar(_coment, _logger);

                // ---------- VALIDAR ID ----------
                if (_coment.Id <= 0)
                {
                    _logger.LogWarning("ID de comentario inválido: {Id}", _coment.Id);
                    throw new ArgumentException("El ID del comentario es inválido.");
                }

                // ---------- OBTENER DE BD ----------
                var comentarioBD = await _contextoBD.ComentarioTarea
                    .FirstOrDefaultAsync(c => c.Id == _coment.Id);

                if (comentarioBD == null)
                {
                    _logger.LogWarning("No se encontró un comentario con ID {Id} para editar.", _coment.Id);
                    return false;
                }

                // ---------- ACTUALIZAR SOLO CAMPOS PERMITIDOS ----------
                comentarioBD.Comentario = _coment.Comentario;
                comentarioBD.FechaModificacion = DateTime.UtcNow;

                // ---------- GUARDAR EN BD ----------
                await _contextoBD.SaveChangesAsync();

                _logger.LogInformation("Comentario {Id} editado correctamente.", _coment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el comentario con ID {Id}.", _coment?.Id);
                throw;
            }
        }

    }
}