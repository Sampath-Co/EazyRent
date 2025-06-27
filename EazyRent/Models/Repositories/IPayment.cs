using EazyRent.Models.DTO;
using EazyRent.Models.Domains;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EazyRent.Models.Repositories
{
    public interface IPayment
    {
        Task<IEnumerable<PaymentDTO>> GetPaymentsByLeaseIdAsync(int leaseId);
        Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId);
        //Task<bool> AddPaymentAsync(PaymentDTO paymentDto);
        Task<Payment> AddPaymentAsync(Payment payment);

        //Task<Payment> AddPaymentAsync(Payment payment);
        //Task<Payment> GetPaymentByIdAsync(int paymentId);
        //Task<bool> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentsByLeaseIdAsync(int leaseId);
    }
}