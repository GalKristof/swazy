using System;

namespace Api.Swazy.Models.DTOs.BusinessServices
{
    public class CreateBusinessServiceDto
    {
        public Guid BusinessId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
    }
}
