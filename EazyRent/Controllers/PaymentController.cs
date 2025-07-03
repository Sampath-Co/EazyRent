using AutoMapper;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayment _paymentRepository;
        private readonly ILease _leaseRepository;
        private readonly IMapper _mapper;

        public PaymentController(IPayment paymentRepository, ILease leaseRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _leaseRepository = leaseRepository;
            _mapper = mapper;
        }

        // 1. Get Payments for Owner and Tenant
        [HttpGet("/Payments")]
        [Authorize(Roles = "Tenant,Owner")]
        public async Task<IActionResult> GetPayments()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { Message = "User ID claim not found or is invalid." });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            IEnumerable<PaymentDTO> payments;
            if (role == "Owner")
            {
                payments = await _paymentRepository.GetPaymentsForOwnerAsync(userId);
            }
            else if (role == "Tenant")
            {
                payments = await _paymentRepository.GetPaymentsForTenantAsync(userId);
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "You are not authorized to view payments." });
            }

            if (payments == null || !payments.Any())
            {
                return NoContent();
            }

            return Ok(payments);
        }

        // 2. Update Payment Status to "Paid" (Tenant Only)
        [HttpPut("/Tenant/UpdatePaymentStatus/{leaseId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> UpdatePaymentStatus(int leaseId)
        {
            var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
            {
                return Unauthorized(new { Message = "Tenant ID claim not found or is invalid." });
            }

            // Verify that the lease belongs to the tenant
            var lease = await _leaseRepository.GetLeaseByIdAsync(leaseId);
            if (lease == null || lease.TenantId != tenantId)
            {
                return NotFound(new { Message = "Lease not found or you are not authorized to update this payment." });
            }

            // Update payment status
            var success = await _paymentRepository.UpdatePaymentStatusAsync(leaseId, "Paid");
            if (!success)
            {
                return BadRequest(new { Message = "Failed to update payment status." });
            }

            return Ok(new { Message = "Payment status updated to 'Paid' successfully." });
        }

        [HttpGet("/Lease/{leaseId}/Payments")]
        //[Authorize(Roles = "Tenant,Owner")]
        public async Task<IActionResult> GetPaymentsByLeaseId(int leaseId)
        {
            try
            {
                var payments = await _paymentRepository.GetPaymentsByLeaseIdAsync(leaseId);

                if (payments == null || !payments.Any())
                {
                    return NoContent();
                }

                var paymentDtos = _mapper.Map<IEnumerable<PaymentDTO>>(payments);
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while fetching payments.", Details = ex.Message });
            }
        }

        [HttpGet("/{paymentId}")]
        //[Authorize(Roles = "Tenant,Owner")]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            var paymentDto = _mapper.Map<PaymentDTO>(payment);
            return Ok(paymentDto);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentDTO paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payment = _mapper.Map<Payment>(paymentDto);
                payment.Status = "Pending"; // Default status
                //payment.CreatedDate = DateTime.UtcNow;

                var createdPayment = await _paymentRepository.AddPaymentAsync(payment);

                if (createdPayment == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to create payment." });
                }

                var createdDto = _mapper.Map<PaymentDTO>(createdPayment);
                return CreatedAtAction(nameof(GetPaymentById), new { paymentId = createdDto.LeaseId }, createdDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error creating payment.", Details = ex.Message });
            }
        }


        //[HttpPost]
        ////[Authorize(Roles = "Tenant")]
        //public async Task<IActionResult> AddPayment([FromBody] PaymentDTO paymentDto)
        //{
        //    var payment = _mapper.Map<Payment>(paymentDto);
        //    var success = await _paymentRepository.AddPaymentAsync(payment);

        //    if (!success)
        //    {
        //        return BadRequest("Failed to add payment.");
        //    }

        //    return Ok("Payment added successfully.");
        //}
    }
}