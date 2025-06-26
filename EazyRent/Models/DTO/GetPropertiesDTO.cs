public class GetPropertiesDTO
{
    public int PropertyId { get; set; }
    public string? Address { get; set; }
    public decimal? RentAmount { get; set; }
    public string? AvailabilityStatus { get; set; }
    public string? PropertyImageBase64 { get; set; } // New property
    public string? PropertyDescription { get; set; }
}
