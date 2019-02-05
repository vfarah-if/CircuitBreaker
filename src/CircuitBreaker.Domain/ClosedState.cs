using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitBreaker.Domain
{
    public class ClosedState : CircuitBreakerState
    {
        public ClosedState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            circuitBreaker.ResetFailureCount();
        }

        public override void ActUponException(Exception e)
        {
            base.ActUponException(e);
            if (circuitBreaker.IsThresholdReached())
            {
                circuitBreaker.MoveToOpenState();
            }
        }
    }
}
