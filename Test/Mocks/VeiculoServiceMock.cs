using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Infraestrutura;

namespace Test.Mocks
{
    public class VeiculoServiceMock : IVeiculoServico
    {
        private static List<Veiculo> veiculos = new List<Veiculo>(){
            new() {
                Id = 1,
                Nome = "X6",
                Marca = "BMW",
                Ano = 2024
            },
            new() {
                Id = 2,
                Nome = "Fiat Toro",
                Marca = "Fiat",
                Ano = 2023
            }
        };

        public void Apagar(Veiculo veiculo)
        {
            var carro = veiculos.Find(v => v.Id == veiculo.Id);
            veiculos.Remove(carro);
        }

        public Veiculo Atualizar(Veiculo veiculo)
        {
            var veiculoExistente = veiculos.Find(v => v.Id == veiculo.Id);

            if (veiculoExistente != null)
            {
                veiculoExistente.Nome = veiculo.Nome;
                veiculoExistente.Marca = veiculo.Marca;
                veiculoExistente.Ano = veiculo.Ano;
            }
            
            return veiculo;
        }

        public Veiculo? BuscaPorId(int id)
        {
            return veiculos.Find(v => v.Id == id);
        }

        public Veiculo Incluir(Veiculo veiculo)
        {
            veiculo.Id = veiculos.Count() + 1;
            veiculos.Add(veiculo);

            return veiculo;
        }

        public List<Veiculo> Todos(int? page = 1, string? nome = null, string? marca = null)
        {
            var query = veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => v.Nome.ToLower().Contains(nome.ToLower()));
            }

            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(v => v.Marca.ToLower().Contains(marca.ToLower()));
            }

            int itensPorPagina = 10;

            return query
                .Skip((page ?? 1 - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();
        }
    }
}