using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using AuthBackend.Services;
using Microsoft.Extensions.Logging;

namespace AuthBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RabbitMQProducer _producer;
        private readonly ILogger<AuthController> _logger;
        private static readonly ConcurrentDictionary<string, string> _emailCodes = new ConcurrentDictionary<string, string>();

        public AuthController(RabbitMQProducer producer, ILogger<AuthController> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        [HttpPost("send-code")]
        public IActionResult SendCode([FromBody] EmailRequest request)
        {
            var code = new Random().Next(1000, 9999).ToString();
            _emailCodes[request.Email] = code;
            _producer.SendEmail(request.Email, code); // ���������� ��������� � �������
            _logger.LogInformation($"[{DateTime.Now}] ��� {code} ������������ ��� {request.Email}");
            return Ok(new { message = "��� ��������� �� ���� �����." });
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
        {
            if (_emailCodes.TryGetValue(request.Email, out var savedCode) && savedCode == request.Code)
            {
                _emailCodes.TryRemove(request.Email, out _);
                _logger.LogInformation($"[{DateTime.Now}] ��� {request.Code} ������� ����������� ��� {request.Email}");
                return Ok(new { message = "��� ������!", success = true });
            }
            else
            {
                _logger.LogWarning($"[{DateTime.Now}] ������ �����������: �������� ��� ��� {request.Email}");
                return BadRequest(new { message = "�������� ���." });
            }
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
    }

    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
