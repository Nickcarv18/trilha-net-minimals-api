using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Test.Helpers;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.Enums;
using trilha_net_minimals_api.Dominio.ModelViews;

namespace Test.Request
{
    [TestClass]
    public class AdministradorRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext){
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            Setup.ClassCleanup();
        }

        [TestMethod]
        public async Task TestarRequestLogin()
        {
            // Arrange
            var loginDto = new LoginDTO{
                Email = "adm@teste.com",
                Senha = "123456"
            };
            var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "Application/json");

            //Act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result  = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado> (result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");
        }

        [TestMethod]
        public async Task TestarIncluirAdministrador()
        {
            // Arrange
            var novoAdm = new AdministradorDTO
            {
                Email = "novoadm@teste.com",
                Perfil = Perfil.Adm,
                Senha = "senhaSegura"
            };

            var loginDto = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            // Act (Login)
            var contentLogin = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "Application/json");
            var responseLogin = await Setup.client.PostAsync("/administradores/login", contentLogin);

            // Extrai o token do usuário logado
            var resultLogin = await responseLogin.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(resultLogin, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var token = admLogado?.Token;

            // Inclui header de autorização
            var client = Setup.client;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonNovoAdm = JsonSerializer.Serialize(novoAdm);
            var contentAdm = new StringContent(jsonNovoAdm, Encoding.UTF8, "Application/json");

            // Act (Cria novo adminstrador)
            var responseIncluir = await client.PostAsync("/administradores", contentAdm);

            // Assert
            var resultIncluir = await responseIncluir.Content.ReadAsStringAsync();
            var admCriado = JsonSerializer.Deserialize<Administrador>(resultIncluir, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admCriado?.Email ?? "");
            Assert.IsNotNull(admCriado?.Perfil ?? "");
        }

        [TestMethod]
        public async Task TestarBuscarPorIdAdministrador()
        {
            // Arrange
            int administradorId = 1;
            var loginDto = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            // Act (Login)
            var contentLogin = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "Application/json");
            var responseLogin = await Setup.client.PostAsync("/administradores/login", contentLogin);

            // Extrai o token do usuário logado
            var resultLogin = await responseLogin.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(resultLogin, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var token = admLogado?.Token;

            // Inclui header de autorização
            var client = Setup.client;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act (Busca por ID)
            var responseBuscaPorid = await client.GetAsync($"/administradores/{administradorId}");
            var resultBuscaPorid = await responseBuscaPorid.Content.ReadAsStringAsync();

            if (responseBuscaPorid.IsSuccessStatusCode)
            {
                var administrador = JsonSerializer.Deserialize<AdministradorModelView>(resultBuscaPorid, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Assert
                Assert.IsNotNull(administrador.Email);
                Assert.IsNotNull(administrador.Perfil);
            }
        }

        [TestMethod]
        public async Task TestarApagarAdministrador()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var novoAdm = new AdministradorDTO
            {
                Email = "novoadm@teste.com",
                Perfil = Perfil.Adm,
                Senha = "senhaSegura"
            };

            // Act (Login)
            var contentLogin = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "Application/json");
            var responseLogin = await Setup.client.PostAsync("/administradores/login", contentLogin);

            // Extrai o token do usuário logado
            var resultLogin = await responseLogin.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(resultLogin, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var token = admLogado?.Token;

            // Inclui header de autorização
            var client = Setup.client;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act (Cria novo adminstrador)
            var jsonNovoAdm = JsonSerializer.Serialize(novoAdm);
            var contentAdm = new StringContent(jsonNovoAdm, Encoding.UTF8, "Application/json");
            var responseIncluir = await client.PostAsync("/administradores", contentAdm);
            var resultIncluir = await responseIncluir.Content.ReadAsStringAsync();
            var admCriado = JsonSerializer.Deserialize<Administrador>(resultIncluir, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Pega o id do administrador
            var responseBuscaPorid = await client.GetAsync($"/administradores/{admCriado.Id}");
            var resultBuscaPorid = await responseBuscaPorid.Content.ReadAsStringAsync();
            var adminParaApagar = JsonSerializer.Deserialize<AdministradorModelView>(resultBuscaPorid, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act (Delete administrador)
            if (adminParaApagar != null)
            {
                var responseApagar = await client.DeleteAsync($"/administradores/{adminParaApagar.Id}");

                // Assert
                Assert.AreEqual(HttpStatusCode.NoContent, responseApagar.StatusCode);
            }
        }
    }
}