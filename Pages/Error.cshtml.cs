using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace NLI_POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {      
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;
        private readonly IEmailSender _emailSender;

        public ErrorModel(ILogger<ErrorModel> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                var exception = exceptionFeature.Error;

                // Log it
                _logger.LogError(exception, "Unhandled exception occurred. ID: {RequestId}", RequestId);

                // Email tech support
                var message = $@"
                An unhandled exception occurred.

                Request ID: {RequestId}
                Path: {HttpContext.Request.Path}
                Error Message: {exception.Message}
                Stack Trace:
                {exception.StackTrace}
            ";

                // This should ideally be done asynchronously via background queue
                _emailSender.SendEmailAsync("armand.nlip@gmail.com", "Unhandled Exception", message);
            }
        }
    }

}
