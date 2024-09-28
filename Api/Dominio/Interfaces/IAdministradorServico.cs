using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;

namespace trilha_net_minimals_api.Infraestrutura
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        void Incluir(Administrador administrador);
        Administrador? BuscaPorId(int id);
        List<Administrador> Todos(int? page = 1);
        void Apagar(Administrador administrador);
    }
}