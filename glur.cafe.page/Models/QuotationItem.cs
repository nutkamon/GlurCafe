using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class QuotationItem
    {
        public int Id { get; set; }

        public int QuotationId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรายการ")]
        [StringLength(200)]
        [Display(Name = "รายการ")]
        public string Description { get; set; } = "";

        [Display(Name = "จำนวน")]
        public int Quantity { get; set; } = 1;

        [StringLength(20)]
        [Display(Name = "หน่วย")]
        public string Unit { get; set; } = "ชิ้น";

        [Display(Name = "ราคาต่อหน่วย")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "รวม")]
        public decimal Amount { get; set; }

        public Quotation? Quotation { get; set; }
    }
}
