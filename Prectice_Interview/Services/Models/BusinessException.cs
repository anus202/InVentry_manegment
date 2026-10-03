using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}
