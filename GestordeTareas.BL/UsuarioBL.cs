using GestordeTaras.EN;
using GestordeTareas.DAL;
using GestordeTareas.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestordeTareas.BL
{
    public class UsuarioBL
    {
        private readonly IUsuarioDAL _UsuarioDAL;

        public UsuarioBL(IUsuarioDAL usuarioDAL)
        {
            _UsuarioDAL = usuarioDAL;
        }
        public async Task<int> Create(Usuario usuario)
        {
            return await _UsuarioDAL.CreateAsync(usuario);
        }

        public async Task<int> Update(Usuario usuario)
        {
            return await _UsuarioDAL.UpdateAsync(usuario);
        }

        public async Task<int> Delete(Usuario usuario)
        {
            return await _UsuarioDAL.DeleteAsync(usuario);
        }

        public async Task<Usuario> GetByIdAsync(Usuario usuario)
        {
            return await _UsuarioDAL.GetByIdAsync(usuario);
        }

        // Método para obtener un usuario por su nombre de usuario
        public async Task<Usuario> GetByNombreUsuarioAsync(Usuario usuario)
        {
            return await _UsuarioDAL.GetByNombreUsuarioAsync(usuario);
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _UsuarioDAL.GetAllAsync();
        }

        public async Task<List<Usuario>> SearchAsync(Usuario usuarios)
        {
            return await _UsuarioDAL.SearchAsync(usuarios);
        }

        public async Task<List<Usuario>> SearchIncludeRoleAsync(Usuario user, string query, string filter)
        {
            return await _UsuarioDAL.SearchIncludeRoleAsync(user, query, filter);
        }

        public async Task<Usuario> LoginAsync(Usuario usuarios)
        {
            return await _UsuarioDAL.LoginAsync(usuarios);
        }

        // Método para generar un token de restablecimiento de contraseña
        public async Task<int> GenerarCodigoRestablecimientoAsync(Usuario usuario)
        {
            return await _UsuarioDAL.GenerarCodigoRestablecimientoAsync(usuario);
        }

        // Método para validar un token de restablecimiento de contraseña
        public async Task<bool> ValidarCodigoRestablecimientoAsync(int Idusuario, string codigo)
        {
            return await _UsuarioDAL.ValidarCodigoRestablecimientoAsync(Idusuario, codigo);
        }

        // Método para restablecer la contraseña del usuario usando un token
        public async Task<int> RestablecerContrasenaAsync(int Idusuario, string codigo, string nuevaContrasena)
        {
            return await _UsuarioDAL.RestablecerContrasenaAsync(Idusuario, codigo, nuevaContrasena);
        }

    }
}