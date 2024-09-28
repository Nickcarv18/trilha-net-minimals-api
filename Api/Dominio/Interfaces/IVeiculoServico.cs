using trilha_net_minimals_api.Dominio.Entidades;

namespace trilha_net_minimals_api.Infraestrutura
{
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int? page = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaPorId(int id);

        void Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void Apagar(Veiculo veiculo);
    }
}