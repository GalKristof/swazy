using System;
using Api.Swazy.Models.DTOs;

namespace Api.Swazy.Models.DTOs.BusinessServices
{
    public class UpdateBusinessServiceDto : BaseUpdateDto
    {
        public decimal? Price { get; set; }
        public int? Duration { get; set; }
    }
}
