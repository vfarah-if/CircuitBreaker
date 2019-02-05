using System;

namespace CircuitBreaker.Domain
{
    public class HealthyClosedState : CircuitBreakerState
    {
        public HealthyClosedState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            circuitBreaker.ResetFailureCount();
        }

        internal override void OnError(Exception e)
        {
            base.OnError(e);
            if (circuitBreaker.IsThresholdReached())
            {
                circuitBreaker.MoveToBrokenState();
            }
        }
    }
}
