using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Configs
{
    public class RabbitMqConfig
    {
        public required string HostName { get; init; }
        public required string UserName { get; init; }
        public required string Password { get; init; }
        public int Port { get; init; }
    }
}