using AutoMapper;
using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EazyRent.Models.Repositories
{
    public class PaymentRepository : IPayment
    {
        private readonly RentalDBContext _dbContext;
        private readonly IMapper _mapper;

        public PaymentRepository(RentalDBContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentDTO>> GetPaymentsByLeaseIdAsync(int leaseId)
        {
            var payments = await _dbContext.Payments
                .Where(p => p.LeaseId == leaseId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<bool> AddPaymentAsync(PaymentDTO paymentDto)
        {
            var payment = _mapper.Map<Payment>(paymentDto);
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
            return payment; 
        }
    }
}