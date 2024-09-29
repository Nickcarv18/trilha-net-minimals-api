using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Infraestrutura;

namespace Test.Mocks
{
    public class AdministradorServiceMock : IAdministradorServico
    {
        private static List<Administrador> administradores = new List<Administrador>(){
            new() {
                Id = 1,
                Email = "adm@teste.com",
                Perfil = "Adm",
                Senha = "123456"
            },
            new() {
                Id = 2,
                Email = "editor@teste.com",
                Perfil = "123456",
                Senha = "Editor"
            }
        };

        public void Apagar(Administrador administrador)
        {
            var adm = administradores.Find(a => a.Id == administrador.Id);
            administradores.Remove(adm);
        }

        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(a => a.Id == id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count() + 1;
            administradores.Add(administrador);

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return administradores.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int? page = 1)
        {
            return administradores;
        }
    }
}