using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trilha_net_minimals_api.Dominio.DTOs;
using trilha_net_minimals_api.Dominio.ModelViews;
using trilha_net_minimals_api.Dominio.Servicos;
using trilha_net_minimals_api.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorService>();

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/", () => Results.Json(new Home()));

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO) != null){
        return Results.Ok("Login com sucesso!");
    }else{
        return Results.Unauthorized();
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();