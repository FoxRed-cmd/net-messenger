using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Entities.DTO
{
    public class UserRegisteredEvent
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
    }
}