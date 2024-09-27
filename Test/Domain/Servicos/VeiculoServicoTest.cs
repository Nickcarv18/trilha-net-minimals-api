using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using trilha_net_minimals_api.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.Servicos;
using System.Reflection;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class VeiculoServicoTest
    {
        private DbContexto CriarContextoDeTeste(){
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

           //Configurar o ConfigurationBuilder
            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestandoIncluirVeiculo()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            var veiculo = new Veiculo();
            veiculo.Nome = "Honda Fit";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2022;

            var veiculoServico = new VeiculoService(context);

            //Act
            veiculoServico.Incluir(veiculo);

            //Assert
            Assert.AreEqual(1, veiculoServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestandoBuscaPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            var veiculo = new Veiculo();
            veiculo.Nome = "Honda Fit";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2022;

            var veiculoServico = new VeiculoService(context);
            veiculoServico.Incluir(veiculo);

            //Act
            var veiculoTest = veiculoServico.BuscaPorId(veiculo.Id);

            //Assert
            Assert.AreEqual(1, veiculoTest.Id);
        }

        [TestMethod]
        public void TestandoAtualizarVeiculo()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            var veiculo = new Veiculo();
            veiculo.Nome = "Honda Fit";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2022;

            var veiculoServico = new VeiculoService(context);
            veiculoServico.Incluir(veiculo);

            //Act
            veiculo.Nome = "Honda Fit Atualizado";
            veiculo.Ano = 2023;
            veiculoServico.Atualizar(veiculo);
            
            var veiculoBanco = veiculoServico.BuscaPorId(veiculo.Id);

            //Assert
            Assert.AreEqual("Honda Fit Atualizado", veiculoBanco.Nome);
            Assert.AreEqual(2023, veiculoBanco.Ano);
            Assert.AreEqual("Honda", veiculoBanco.Marca);
        }

        [TestMethod]
        public void TestandoApagarVeiculo()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            var veiculo = new Veiculo();
            veiculo.Nome = "Honda Fit";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2022;

            var veiculoServico = new VeiculoService(context);
            veiculoServico.Incluir(veiculo);

            //Act
            veiculoServico.Apagar(veiculo);
            var veiculoApagado = veiculoServico.BuscaPorId(veiculo.Id);

            //Assert
            Assert.IsNull(veiculoApagado, "O veículo não foi apagado corretamente.");
        }
    }
}