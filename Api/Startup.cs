using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.Enums;
using trilha_net_minimals_api.Dominio.ModelViews;
using trilha_net_minimals_api.Dominio.Servicos;
using trilha_net_minimals_api.Infraestrutura;

namespace trilha_net_minimals_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = Configuration.GetValue<string>("Jwt") ?? "";
        }

        private string key;
        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(option => {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option => {
                option.TokenValidationParameters = new TokenValidationParameters{
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdministradorServico, AdministradorService>();
            services.AddScoped<IVeiculoServico, VeiculoService>();

            services.AddDbContext<DbContexto>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"))
            );

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT aqui: "
                });

                options.AddSecurityRequirement( new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference 
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }
    
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                #region Home
                    endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administradores
                    ErrosDeValidacao validaAdministradorDTO(AdministradorDTO administradorDTO)
                    {
                        var validacao = new ErrosDeValidacao{
                            Mensagens = new List<string>()
                        };

                        if(string.IsNullOrEmpty(administradorDTO.Email)){
                            validacao.Mensagens.Add("O email não pode ficar em branco.");
                        }

                        if(string.IsNullOrEmpty(administradorDTO.Senha)){
                            validacao.Mensagens.Add("A senha não pode ficar em branco.");
                        }

                        if(administradorDTO.Perfil == null){
                            validacao.Mensagens.Add("O perfil não pode ficar em branco.");
                        }

                        return validacao;
                    }

                    string GerarTokeJwt(Administrador administrador){
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Email, administrador.Email),
                            new Claim(ClaimTypes.Role, administrador.Perfil),
                            new Claim("Perfil", administrador.Perfil)
                        };
                        
                        var token = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: credentials
                        );

                        return new JwtSecurityTokenHandler().WriteToken(token);
                    }

                    endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
                        var adm = administradorServico.Login(loginDTO);

                        if(adm != null){
                            string token = GerarTokeJwt(adm);
                            return Results.Ok(new AdministradorLogado {
                                Email = adm.Email,
                                Perfil = adm.Perfil,
                                Token = token
                            });
                        }else{
                            return Results.Unauthorized();
                        }
                    }).AllowAnonymous().WithTags("Administradores");

                    endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
                        var validacao = validaAdministradorDTO(administradorDTO);
                        if(validacao.Mensagens.Count > 0){
                            return Results.BadRequest(validacao);
                        }

                        var administrador = new Administrador{
                            Email = administradorDTO.Email,
                            Senha = administradorDTO.Senha,
                            Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                        };

                        administradorServico.Incluir(administrador);

                        return Results.Created("/administradores/{id}", new AdministradorModelView{
                            Id = administrador.Id,
                            Email = administrador.Email,
                            Perfil = administrador.Perfil
                        });
                    }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Administradores");

                    endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
                        var adms = new List<AdministradorModelView>();
                        var administradores = administradorServico.Todos(pagina);

                        foreach (var adm in administradores)
                        {
                                adms.Add(new AdministradorModelView{
                                    Id = adm.Id,
                                    Email = adm.Email,
                                    Perfil = adm.Perfil
                                });
                        }
                    return Results.Ok(adms);
                    }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Administradores");

                    endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                        var administrador = administradorServico.BuscaPorId(id);

                        if(administrador == null) return Results.NotFound();

                        return Results.Ok(new AdministradorModelView{
                                Id = administrador.Id,
                                Email = administrador.Email,
                                Perfil = administrador.Perfil
                            });
                    }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Administradores");

                    endpoints.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                    var administrador = administradorServico.BuscaPorId(id);
                    if(administrador == null) return Results.NotFound();

                    administradorServico.Apagar(administrador);

                    return Results.NoContent();
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Administradores");
                #endregion

                #region Veiculos
                    ErrosDeValidacao validaDTO(VeiculoDTO veiculoDto)
                {
                    var validacao = new ErrosDeValidacao{
                        Mensagens = new List<string>()
                    };

                    if(string.IsNullOrEmpty(veiculoDto.Nome)){
                        validacao.Mensagens.Add("O nome não pode ficar em branco.");
                    }

                    if(string.IsNullOrEmpty(veiculoDto.Marca)){
                        validacao.Mensagens.Add("A marca não pode ficar em branco.");
                    }

                    if(veiculoDto.Ano < 1950){
                        validacao.Mensagens.Add("Veículo muito antigo, aceito somente anos superiores a 1950.");
                    }

                    return validacao;
                }

                    endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) => {
                        var validacao = validaDTO(veiculoDto);
                        if(validacao.Mensagens.Count > 0){
                            return Results.BadRequest(validacao);
                        }

                        var veiculo = new Veiculo{
                            Nome = veiculoDto.Nome,
                            Marca = veiculoDto.Marca,
                            Ano = veiculoDto.Ano
                        };

                        veiculoServico.Incluir(veiculo);

                        return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm, Editor"})
                    .WithTags("Veiculos");

                    endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
                    var veiculos = veiculoServico.Todos(pagina);
                    return Results.Ok(veiculos);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm, Editor"})
                    .WithTags("Veiculos");

                    endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if(veiculo == null) return Results.NotFound();

                    return Results.Ok(veiculo);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm, Editor"})
                    .WithTags("Veiculos");

                    endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) => {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if(veiculo == null) return Results.NotFound();

                        var validacao = validaDTO(veiculoDto);
                        if(validacao.Mensagens.Count > 0){
                            return Results.BadRequest(validacao);
                        }

                        veiculo.Nome = veiculoDto.Nome;
                        veiculo.Marca = veiculoDto.Marca;
                        veiculo.Ano = veiculoDto.Ano;

                        veiculoServico.Atualizar(veiculo);

                        return Results.Ok(veiculo);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
                    .WithTags("Veiculos");

                    endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if(veiculo == null) return Results.NotFound();

                    veiculoServico.Apagar(veiculo);

                    return Results.NoContent();
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
                    .WithTags("Veiculos");
                #endregion
            });
        }
    }
}