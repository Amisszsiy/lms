using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ITokenService
    {
        public string Create(User user, IEnumerable<string> roles);
    }
}
