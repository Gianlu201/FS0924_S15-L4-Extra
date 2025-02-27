namespace FS0924_S15_L4_Extra.Models
{
    public class Employee
    {
        public Guid? IdEmployee { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? FiscalCode { get; set; }
        public int? Age { get; set; }
        public decimal? Income { get; set; }
        public bool? TaxDeduction { get; set; }
        public string? Employment { get; set; }
    }
}
