using System;

namespace CircuitBreaker.Domain
{
    public class BrokenOpenState : CircuitBreakerState
    {
        private readonly DateTime openDateTime;

        public BrokenOpenState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            openDateTime = DateTime.UtcNow;
            circuitBreaker.BeforeInvoke += OnBeforeInvoke;
        }

        private void OnBeforeInvoke(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= openDateTime + base.circuitBreaker.Timeout)
            {
                circuitBreaker.MoveToMendingState();
            }
        }
    }
}