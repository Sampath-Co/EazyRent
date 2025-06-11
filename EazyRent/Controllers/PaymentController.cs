using AutoMapper;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayment _paymentRepository;
        private readonly IMapper _mapper;

        public PaymentController(IPayment paymentRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
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

        [HttpPost]
        //[Authorize(Roles = "Tenant")]
        public async Task<IActionResult> AddPayment([FromBody] PaymentDTO paymentDto)
        {
            var payment = _mapper.Map<Payment>(paymentDto);
            var success = await _paymentRepository.AddPaymentAsync(payment);

            if (!success)
            {
                return BadRequest("Failed to add payment.");
            }

            return Ok("Payment added successfully.");
        }
    }
}