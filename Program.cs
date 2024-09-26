using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.Enums;
using trilha_net_minimals_api.Dominio.ModelViews;
using trilha_net_minimals_api.Dominio.Servicos;
using trilha_net_minimals_api.Infraestrutura;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetValue<string>("Jwt");
if(string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorService>();
builder.Services.AddScoped<IVeiculoServico, VeiculoService>();

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
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
        new Claim("Perfil", administrador.Perfil)
    };
    
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
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
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
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
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
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
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
   var administrador = administradorServico.BuscaPorId(id);

   if(administrador == null) return Results.NotFound();

   return Results.Ok(new AdministradorModelView{
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization().WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
   var administrador = administradorServico.BuscaPorId(id);
   if(administrador == null) return Results.NotFound();

   administradorServico.Apagar(administrador);

   return Results.NoContent();
}).RequireAuthorization().WithTags("Administradores");
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

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) => {
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
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
   var veiculos = veiculoServico.Todos(pagina);
   return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
   var veiculo = veiculoServico.BuscaPorId(id);
   if(veiculo == null) return Results.NotFound();

   return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) => {
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
}).RequireAuthorization().WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
   var veiculo = veiculoServico.BuscaPorId(id);
   if(veiculo == null) return Results.NotFound();

   veiculoServico.Apagar(veiculo);

   return Results.NoContent();
}).RequireAuthorization().WithTags("Veiculos");
#endregion

#region App
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion