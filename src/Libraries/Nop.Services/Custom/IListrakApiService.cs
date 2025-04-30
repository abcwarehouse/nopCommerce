using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Custom;

namespace Nop.Services.Custom
{
    public interface IListrakApiService
    {
        Task<string> GetTokenAsync();
        ApiResponse CheckSubList(String phoneNumber);
    }
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}