using System;
using System.Collections.Generic;

namespace AcuarioWebs.Models;

public partial class Atencione
{
    public int IdAtencion { get; set; }

    public string TipoActividad { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int IdUser { get; set; }

    public int IdPeces { get; set; }

    public int? IdAlimento { get; set; }

    public virtual Alimentoo? IdAlimentoNavigation { get; set; }

    public virtual Pece? IdPecesNavigation { get; set; } = null!;

    public virtual Usuario? IdUserNavigation { get; set; } = null!;
}
