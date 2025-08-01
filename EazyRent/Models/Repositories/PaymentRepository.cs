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
                .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
                .Where(p => p.LeaseId == leaseId)
                .ToListAsync();

            return payments.Select(p => new PaymentDTO
            {
                LeaseId = p.LeaseId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                TenantName = p.Lease?.Tenant?.FullName
            });
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return null;

            return new PaymentDTO
            {
                LeaseId = payment.LeaseId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                TenantName = payment.Lease?.Tenant?.FullName
            };
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

        public async Task<bool> DeletePaymentsByLeaseIdAsync(int leaseId)
        {
            var payments = await _dbContext.Payments
                                     .Where(p => p.LeaseId == leaseId)
                                     .ToListAsync();
            if (payments.Any())
            {
                _dbContext.Payments.RemoveRange(payments);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // New Method: Get Payments for Owner
        public async Task<IEnumerable<PaymentDTO>> GetPaymentsForOwnerAsync(int ownerId)
        {
            var payments = await _dbContext.Payments
                .Include(p => p.Lease)
                .ThenInclude(l => l.Property)
                .Where(p => p.Lease.Property.OwnerId == ownerId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        // New Method: Get Payments for Tenant
        public async Task<IEnumerable<PaymentDTO>> GetPaymentsForTenantAsync(int tenantId)
        {
            var payments = await _dbContext.Payments
                .Include(p => p.Lease)
                .Where(p => p.Lease.TenantId == tenantId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        // New Method: Update Payment Status
        public async Task<bool> UpdatePaymentStatusAsync(int leaseId, string status)
        {
            var payments = await _dbContext.Payments
                .Where(p => p.LeaseId == leaseId)
                .ToListAsync();

            if (!payments.Any())
            {
                return false;
            }

            foreach (var payment in payments)
            {
                payment.Status = status;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}