using trilha_net_minimals_api.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.Servicos;
using System.Reflection;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
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
        public void TestandoSalvaAdministrador()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "Teste@123";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorService(context);

            //Act
            administradorServico.Incluir(adm);

            //Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestandoBuscaPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "Teste@123";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorService(context);
            administradorServico.Incluir(adm);

            //Act
            var admTest = administradorServico.BuscaPorId(adm.Id);

            //Assert
            Assert.AreEqual(1, admTest.Id);
        }

        [TestMethod]
        public void TestandoApagarAdministrador()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "Teste@123";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorService(context);
            administradorServico.Incluir(adm);

            //Act
            administradorServico.Apagar(adm);
            var admApagado = administradorServico.BuscaPorId(adm.Id);

            //Assert
            Assert.IsNull(admApagado, "O administrador não foi apagado corretamente.");
        }
    }
}