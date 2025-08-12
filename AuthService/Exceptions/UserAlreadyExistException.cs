using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Exceptions
{
    public class UserAlreadyExistException(string message) : Exception(message);
}