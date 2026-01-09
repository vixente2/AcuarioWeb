using System;
using System.Collections.Generic;

namespace AcuarioWebs.Models;

public partial class Peceraa
{
    public int IdPecera { get; set; }

    public string NombrePecera { get; set; } = null!;

    public decimal Litros { get; set; }

    public decimal Temperatura { get; set; }

    public decimal Ph { get; set; }

    public virtual ICollection<Pece> Peces { get; set; } = new List<Pece>();
}
