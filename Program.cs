using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.Entidades;
using trilha_net_minimals_api.Dominio.ModelViews;
using trilha_net_minimals_api.Dominio.Servicos;
using trilha_net_minimals_api.Infraestrutura;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorService>();
builder.Services.AddScoped<IVeiculoServico, VeiculoService>();

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO) != null){
        return Results.Ok("Login com sucesso!");
    }else{
        return Results.Unauthorized();
    }
}).WithTags("Administradores");
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
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
   var veiculos = veiculoServico.Todos(pagina);
   return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
   var veiculo = veiculoServico.BuscaPorId(id);
   if(veiculo == null) return Results.NotFound();

   return Results.Ok(veiculo);
}).WithTags("Veiculos");

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
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
   var veiculo = veiculoServico.BuscaPorId(id);
   if(veiculo == null) return Results.NotFound();

   veiculoServico.Apagar(veiculo);

   return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
#endregion