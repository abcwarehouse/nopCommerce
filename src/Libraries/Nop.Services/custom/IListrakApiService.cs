﻿using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Custom;

namespace Nop.Services.Custom
{
    public interface IListrakApiService
    {
        string GetToken();
        ApiResponse SendBillingAddress(string token, Address billingAddress, bool isCheckboxChecked);
    }
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}