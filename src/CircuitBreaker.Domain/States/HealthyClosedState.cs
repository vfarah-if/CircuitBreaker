using System;

namespace CircuitBreaker.Domain
{
    public class HealthyClosedState : CircuitBreakerState
    {
        public HealthyClosedState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            circuitBreaker.Reset();
            circuitBreaker.Error += OnError;
        }

        private void OnError(Object sender, EventArgs e)
        {
            if (circuitBreaker.IsThresholdReached)
            {
                circuitBreaker.MoveToBrokenState();
            }
        }
    }
}
