using Microsoft.EntityFrameworkCore;
using trilha_net_minimals_api.Dominio.Entidades;

namespace trilha_net_minimals_api.Infraestrutura
{
    public class DbContexto : DbContext
    {

        private readonly IConfiguration _configurationAppSettings;

        public DbContexto(IConfiguration configurationAppSettings)
        {
            _configurationAppSettings = configurationAppSettings;
        }

        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador{
                    Id = 1,
                    Email = "adm@admTeste.com",
                    Senha = "123456",
                    Perfil = "Adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured){
                var stringConexao = _configurationAppSettings.GetConnectionString("SqlServer");

                if(!string.IsNullOrEmpty(stringConexao)){
                    optionsBuilder.UseSqlServer(stringConexao);
                }
            }
        }
    }
}