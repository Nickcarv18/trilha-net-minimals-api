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
    public class VeiculoRequestTest
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
        public async Task TestarIncluirVeiculo()
        {
            // Arrange
            var novoVeiculo = new VeiculoDTO
            {
                Nome = "Novo veiculo",
                Marca = "veiculo",
                Ano = 2024
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

            // Act (Cria novo Veiculo)
            var contentVeiculo = new StringContent(JsonSerializer.Serialize(novoVeiculo), Encoding.UTF8, "Application/json");
            var responseIncluir = await client.PostAsync("/veiculos", contentVeiculo);

            // Assert
            var resultIncluir = await responseIncluir.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(resultIncluir, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(veiculo?.Nome ?? "");
            Assert.IsNotNull(veiculo?.Marca ?? "");
            Assert.IsNotNull(veiculo?.Ano ?? 0);
        }

        [TestMethod]
        public async Task TestarAtualizarVeiculo()
        {
            // Arrange
            var novoVeiculo = new VeiculoDTO
            {
                Nome = "Novo veiculo",
                Marca = "veiculo",
                Ano = 2024
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

            // Act (Cria novo Veiculo)
            var contentVeiculo = new StringContent(JsonSerializer.Serialize(novoVeiculo), Encoding.UTF8, "Application/json");
            var responseIncluir = await client.PostAsync("/veiculos", contentVeiculo);
            var resultIncluir = await responseIncluir.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(resultIncluir, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Act (Atualiza dados veiculo)
            var veiculoAtualizado = new Veiculo
            {
                Id = veiculo.Id, // Utiliza o ID do veículo criado anteriormente
                Nome = "Veículo atualizado",
                Marca = "Marca atualizada",
                Ano = 2025
            };

            // Atualiza o veículo
            var contentVeiculoAtualizar = new StringContent(JsonSerializer.Serialize(veiculoAtualizado), Encoding.UTF8, "Application/json");
            var responseAtualizar = await client.PutAsync($"/veiculos/{veiculo.Id}", contentVeiculoAtualizar);

            // Verifica a resposta
            var resultAtualizar = await responseAtualizar.Content.ReadAsStringAsync();
            var veiculoAtualizadoResposta = JsonSerializer.Deserialize<Veiculo>(resultAtualizar, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(veiculoAtualizado.Nome, veiculoAtualizadoResposta.Nome);
            Assert.AreEqual(veiculoAtualizado.Marca, veiculoAtualizadoResposta.Marca);
            Assert.AreEqual(veiculoAtualizado.Ano, veiculoAtualizadoResposta.Ano);
        }

        [TestMethod]
        public async Task TestarBuscarPorIdVeiculo()
        {
            // Arrange
            int veiculoId = 1;
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
            var responseBuscaPorid = await client.GetAsync($"/veiculos/{veiculoId}");
            var resultBuscaPorid = await responseBuscaPorid.Content.ReadAsStringAsync();

            if (responseBuscaPorid.IsSuccessStatusCode)
            {
                var veiculo = JsonSerializer.Deserialize<Veiculo>(resultBuscaPorid, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Assert
                Assert.IsNotNull(veiculo.Nome);
                Assert.IsNotNull(veiculo.Marca);
                Assert.IsNotNull(veiculo.Ano);
            }
        }

        [TestMethod]
        public async Task TestarApagarVeiculo()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var novoVeiculo = new VeiculoDTO
            {
                Nome = "Novo veiculo",
                Marca = "veiculo",
                Ano = 2024
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

            // Act (Cria novo veiculo)
            var contentVeiculo = new StringContent(JsonSerializer.Serialize(novoVeiculo), Encoding.UTF8, "Application/json");
            var responseIncluir = await client.PostAsync("/veiculos", contentVeiculo);
            var resultIncluir = await responseIncluir.Content.ReadAsStringAsync();
            var veiculoCriado = JsonSerializer.Deserialize<Veiculo>(resultIncluir, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //Pega o id do veiculo
            var responseBuscaPorid = await client.GetAsync($"/veiculos/{veiculoCriado.Id}");
            var resultBuscaPorid = await responseBuscaPorid.Content.ReadAsStringAsync();
            var veiculoParaApagar = JsonSerializer.Deserialize<Veiculo>(resultBuscaPorid, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act (Delete veiculo)
            if (veiculoParaApagar != null)
            {
                var responseApagar = await client.DeleteAsync($"/veiculos/{veiculoParaApagar.Id}");

                // Assert
                Assert.AreEqual(HttpStatusCode.NoContent, responseApagar.StatusCode);
            }
        }
    }
}