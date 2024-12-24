using NorthWind_DataAccessLayer.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentID { get; set; }

    [Required]
    public int MemberID { get; set; }

    [Required]
    [StringLength(20)]
    public string Name { get; set; }

    [Required]
    [StringLength(20)]
    public string CardNumber { get; set; }

    [Required]
    [StringLength(5)]
    public string ExpirationDate { get; set; } // 'MM/YY' formatında saklanır

    [Required]
    [StringLength(3)]
    public string SecurityCode { get; set; }

}