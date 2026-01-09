using System;
using System.Collections.Generic;

namespace AcuarioWebs.Models;

public partial class Pece
{
    public int IdPeces { get; set; }

    public string NombrePez { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public int? Edad { get; set; }

    public int IdPecera { get; set; }

    public virtual ICollection<Atencione> Atenciones { get; set; } = new List<Atencione>();

    public virtual Peceraa? IdPeceraNavigation { get; set; } = null!;
}
