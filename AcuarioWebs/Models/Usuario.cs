using System;
using System.Collections.Generic;

namespace AcuarioWebs.Models;

public partial class Usuario
{
    public int IdUser { get; set; }

    public string Email { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public int IdRol { get; set; }

    public virtual ICollection<Atencione> Atenciones { get; set; } = new List<Atencione>();

    public virtual Rol? IdRolNavigation { get; set; } = null!;
}
