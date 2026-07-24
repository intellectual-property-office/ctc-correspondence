using IPO.CTC.Common.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace IPO.Correspondence.Email
{
    public class Health
    {
        #region Fields and constructors

        private readonly IHealthChecker _healthChecker;

        public Health(IHealthChecker healthChecker)
        {
            _healthChecker = healthChecker;
        }

        #endregion

        [Function(nameof(Live))]
        public IActionResult Live([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/live")] HttpRequest request)
        {
            return _healthChecker.Live();
        }

        [Function(nameof(Ready))]
        public async Task<IActionResult> Ready([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ready")] HttpRequest request)
        {
            return await _healthChecker.Ready();
        }
    }
}