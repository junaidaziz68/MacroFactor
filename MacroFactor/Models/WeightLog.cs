using System;

namespace MacroFactor.Models;

public class WeightLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.Now;
    public double Weight { get; set; }
}
