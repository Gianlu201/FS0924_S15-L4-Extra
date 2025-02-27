using System.ComponentModel.DataAnnotations;

namespace FS0924_S15_L4_Extra.Models
{
    public class EditEmployee
    {
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Surname")]
        public string? Surname { get; set; }

        [Display(Name = "Fiscal code")]
        public string? FiscalCode { get; set; }

        [Display(Name = "Age")]
        public int? Age { get; set; }

        [Display(Name = "Income")]
        public decimal? Income { get; set; }

        [Display(Name = "Tax deduction")]
        public string? TaxDeduction { get; set; }

        [Display(Name = "Employment")]
        public string? Employment { get; set; }

        [Display(Name = "Hiring date")]
        public DateOnly? HiringDate { get; set; }

        public Guid? IdEmployment { get; set; }
        public Guid? IdEmployee { get; set; }
    }
}
