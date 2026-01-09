using System;
using System.Collections.Generic;

namespace AcuarioWebs.Models;

public partial class Alimentoo
{
    public int IdAlimento { get; set; }

    public string NombreAlimento { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual ICollection<Atencione> Atenciones { get; set; } = new List<Atencione>();
}
