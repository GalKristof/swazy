using System;

namespace Api.Swazy.Models.DTOs.BusinessServices
{
    public class BusinessServiceDto
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
    }
}
