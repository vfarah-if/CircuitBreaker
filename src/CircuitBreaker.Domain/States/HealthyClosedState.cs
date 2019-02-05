using System;

namespace CircuitBreaker.Domain
{
    public class HealthyClosedState : CircuitBreakerState
    {
        public HealthyClosedState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            circuitBreaker.Reset();
        }

        internal override void OnError(Exception e)
        {
            if (circuitBreaker.IsThresholdReached())
            {
                circuitBreaker.MoveToBrokenState();
            }
        }
    }
}
