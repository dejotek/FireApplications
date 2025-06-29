// Models/EquivalentApplication.cs
using System;
using System.Collections.Generic;

namespace FireApplications.Models
{
    public class EquivalentApplication
    {
        public string Address       { get; set; } = "";
        public DateTimeOffset EventDate   { get; set; } = DateTimeOffset.Now;
        public string Duration      { get; set; } = "";
        public List<string> Members { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}