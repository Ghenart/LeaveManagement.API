using System;

namespace SimpleApi.Models
{
    public class Holiday
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Örn: "1 Mayıs İşçi Bayramı"
        public DateTime Date { get; set; } // Tatilin tarihi
    }
}