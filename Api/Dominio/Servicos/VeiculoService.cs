using Microsoft.EntityFrameworkCore;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Infraestrutura;

namespace trilha_net_minimals_api.Dominio.Servicos
{
    public class VeiculoService : IVeiculoServico
    {
        private readonly DbContexto _contexto;

        public VeiculoService(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public void Apagar(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> Todos(int? page = 1, string? nome = null, string? marca = null)
        {
           var query =  _contexto.Veiculos.AsQueryable();

           if(!string.IsNullOrEmpty(nome)){
               query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
           }

           if(!string.IsNullOrEmpty(marca)){
               query = query.Where(v => EF.Functions.Like(v.Marca.ToLower(), $"%{marca.ToLower()}%"));
           }

           int itensPorPagina = 10;
           
           if(page != null){
                query = query.Skip(((int)page - 1) * itensPorPagina).Take(itensPorPagina);
           }


           return query.ToList();         
        }
    }
}