using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lumel_SalesAnalysis_Assesment.Models;

[Table("PRODUCTS")]
[Index("ProductCode", Name = "UQ__PRODUCTS__2F4E024F19BA4C11", IsUnique = true)]
public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ProductCode { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string? ProductName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Category { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitCost { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
