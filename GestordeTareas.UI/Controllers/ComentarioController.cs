using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GestordeTareas.UI.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ComentarioController : Controller
    {
        private readonly ILogger<ComentarioController> _logger;
        private readonly IComentarioTareaBL _comentBL;

        public ComentarioController(
            ILogger<ComentarioController> logger,
            IComentarioTareaBL comentBL)
        {
            _logger = logger;
            _comentBL = comentBL;
        }
        [HttpPost("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromBody] ComentarioTarea coment)
        {
            try
            {
                if (coment == null)
                    return BadRequest(new { ok = false, error = "Comentario inválido." });

                // ID usuario (siempre viene del token/cookie)
                var idUsuario = int.Parse(User.FindFirst("IdUsuario").Value);
                coment.IdUsuario = idUsuario;

                var creado = await _comentBL.CrearComentarioAsync(coment);

                return Json(new { ok = true, data = creado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comentario");

                return Json(new { ok = false, error = ex.Message });
            }
        }
        [HttpPost("Editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromBody] ComentarioTarea coment)
        {
            try
            {
                if (coment == null)
                    return BadRequest(new { ok = false, error = "Datos inválidos." });

                var idUsuario = int.Parse(User.FindFirst("IdUsuario").Value);

                var editado = await _comentBL.EditarComentarioAsync(coment, idUsuario);

                return Json(new { ok = editado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar comentario");

                return Json(new { ok = false, error = ex.Message });
            }
        }
        // GET: Obtener comentario por ID
        [HttpGet("Obtener/{idComent}")]
        public async Task<IActionResult> ObtenerComentarioPorIdAsync(int idComent)
        {
            try
            {
                var comentario = await _comentBL.ObtenerComentarioPorIdAsync(idComent);
                if (comentario == null)
                    return NotFound(new { message = "Comentario no encontrado." });

                return Ok(comentario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el comentario con ID {Id}.", idComent);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        // POST: Eliminar comentario (solo el autor)
        [HttpPost("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarComentarioTotalAsync(int idComent, int idUsuarioActual)
        {
            try
            {
                var eliminado = await _comentBL.EliminarComentarioTotalAsync(idComent, idUsuarioActual);
                if (!eliminado)
                    return BadRequest(new { message = "No se pudo eliminar el comentario." });

                return Ok(new { message = "Comentario eliminado correctamente." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Usuario {User} no tiene permiso para eliminar comentario {Id}.", idUsuarioActual, idComent);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el comentario con ID {Id}.", idComent);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }
        // GET: Obtener comentarios raíz de una tarea
        [HttpGet("PorTarea/{idTask}")]
        public async Task<IActionResult> ObtenerComentariosPorTareaAsync(int idTask)
        {
            try
            {
                var comentarios = await _comentBL.ObtenerComentariosPorTareaAsync(idTask);
                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los comentarios para la tarea con ID {Id}.", idTask);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }
        // GET: Obtener hilo completo de comentarios de una tarea (árbol jerárquico)
        [HttpGet("Hilo/{idTask}")]
        public async Task<IActionResult> ObtenerHiloComentariosAsync(int idTask)
        {
            try
            {
                var hilo = await _comentBL.ObtenerHiloComentariosAsync(idTask);
                return Ok(hilo); // lista de comentarios raíz con sus respuestas anidadas
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el hilo de comentarios de la tarea {TaskId}.", idTask);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }
        // GET: Contar comentarios de una tarea
        [HttpGet("Contar/{idTask}")]
        public async Task<IActionResult> ContarComentariosAsync(int idTask)
        {
            try
            {
                var total = await _comentBL.ContarComentariosAsync(idTask);
                return Ok(new { Total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar los comentarios de la tarea {TaskId}.", idTask);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }

    }

}