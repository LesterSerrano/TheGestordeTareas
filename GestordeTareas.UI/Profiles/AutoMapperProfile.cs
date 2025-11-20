using AutoMapper;
using GestordeTareas.UI.DTOs;
using GestordeTaras.EN;
using System;

namespace GestordeTareas.UI.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UsuarioCreateDTO, Usuario>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (byte)User_Status.ACTIVO))
                .ForMember(d => d.FechaRegistro, opt => opt.MapFrom(s => DateTime.Now))
                .ForMember(d => d.ConfirmarPass, opt => opt.Ignore());

            CreateMap<UsuarioEditDTO, Usuario>()
                .ForMember(d => d.Pass, opt => opt.Ignore())
                .ForMember(d => d.FechaRegistro, opt => opt.Ignore());

            CreateMap<Usuario, UsuarioEditDTO>();
        }
    }
} 