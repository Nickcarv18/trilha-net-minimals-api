using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Infraestrutura;

namespace trilha_net_minimals_api.Dominio.Servicos
{
    public class AdministradorService : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public AdministradorService(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores.Where(a=> a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        }

        public List<Administrador> Todos(int? page = 1)
        {
           var query =  _contexto.Administradores.AsQueryable();

           int itensPorPagina = 10;
           
           if(page != null){
                query = query.Skip(((int)page - 1) * itensPorPagina).Take(itensPorPagina);
           }

           return query.ToList();     
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Apagar(Administrador administrador)
        {
            _contexto.Administradores.Remove(administrador);
            _contexto.SaveChanges();
        }
    }
}