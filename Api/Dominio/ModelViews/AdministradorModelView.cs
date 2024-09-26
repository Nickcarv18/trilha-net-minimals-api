using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trilha_net_minimals_api.Dominio.Enums;

namespace trilha_net_minimals_api.Dominio.ModelViews
{
    public record AdministradorModelView
    {
        public int Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}