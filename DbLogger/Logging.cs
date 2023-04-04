using System;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Data;
using financing_api.DbLogger;

namespace financing_api.Logger
{
    public class Logging : ILogging
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Logging(DataContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async void LogTrace(string message)
        {
            LoggingTrace log = new LoggingTrace();

            log.Message = message;

            _context.LoggingTrace.Add(log);
            await _context.SaveChangesAsync();
        }

        public async void LogException(Exception? exception)
        {
            LoggingException log = new LoggingException();

            log.ExceptionMessage = exception.Message;
            log.ExceptionStackTrace = exception.StackTrace;
            log.InnerExceptionMessage = exception.InnerException?.Message;
            log.InnerExceptionStackTrace = exception.InnerException?.StackTrace;

            _context.LoggingException.Add(log);
            await _context.SaveChangesAsync();
        }

        public async void LogDataExchange(string messageSource, string messageTarget, string messagePayload)
        {
            LoggingDataExchange log = new LoggingDataExchange();

            log.MessageSource = messageSource;
            log.MessageTarget = messageTarget;
            log.MessagePayload = messagePayload;

            _context.LoggingDataExchange.Add(log);
            await _context.SaveChangesAsync();

        }
    }
}