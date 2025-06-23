using System;

namespace EazyRent.Models.DTO
{
    public class GetPropertiesDTO
    {
        public string? Address { get; set; }
        public decimal? RentAmount { get; set; }
        public string? AvailabilityStatus { get; set; }
        public byte[]? PropertyImage { get; set; }
        public string? PropertyDescription { get; set; }
    }
}