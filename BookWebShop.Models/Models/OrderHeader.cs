﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.Models.Models;

public class OrderHeader
{
    [Key]
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    [ValidateNever]
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser ApplicationUser { get; set; }
    public DateTime OrderTime { get; set; }
    public DateTime ShippingDate { get; set; }
    public double OrderTotal { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateOnly PaymentDueDate { get; set; }
    public string? SessionId { get; set; }
    public string? PaymentIntentId { get; set; }

    [Required]
    public string Name { get; set; }
    [Required]
    public string Status { get; set; }
    [Required]
    public string StreetAddress { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string PostalCode { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
}
