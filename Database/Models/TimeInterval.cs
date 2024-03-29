﻿
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("TimeIntervals")]
    public class TimeInterval
    {
        [Key]
        public int TimeIntervalId { get; set; }

        [Required]
        public bool IsIndicatorInterval { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public uint Seconds { get; set; }
    }
}
