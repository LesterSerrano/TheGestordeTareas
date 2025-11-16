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
    public class CategoriaBL
    {
        private readonly ICategoriaDAL _CategoriaDAL;

        public CategoriaBL(ICategoriaDAL categoriaDAL)
        {
            _CategoriaDAL = categoriaDAL;
        }
        public async Task<int> CreateAsync(Categoria categoria)
        {
            return await _CategoriaDAL.CreateAsync(categoria);
        }
        public async Task<int> UpdateAsync(Categoria categoria)
        {
            return await _CategoriaDAL.UpdateAsync(categoria);
        }
        public async Task<int> DeleteAsync(Categoria categoria)
        {
            return await _CategoriaDAL.DeleteAsync(categoria);
        }

        public async Task<Categoria> GetByIdAsync(Categoria categoria)
        {
            return await _CategoriaDAL.GetByIdAsync(categoria);
        }
        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _CategoriaDAL.GetAllAsync();
        }
        public async Task<List<Categoria>> SearchAsync(Categoria category)
        {
            return await _CategoriaDAL.SearchAsync(category);
        }
    }
}
