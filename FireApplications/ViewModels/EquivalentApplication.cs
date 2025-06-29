using System;
using System.Collections.Generic;

namespace FireApplications.ViewModels;

public class EquivalentApplication
{
    public string Address       { get; set; }
    public DateTimeOffset EventDate   { get; set; }
    public string Duration      { get; set; }
    public List<string> Members { get; set; }
    public DateTime GeneratedAt { get; set; }
}