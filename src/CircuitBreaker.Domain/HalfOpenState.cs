using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitBreaker.Domain
{
    public class HalfOpenState : CircuitBreakerState
    {
        public HalfOpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }

        public override void ActUponException(Exception e)
        {
            base.ActUponException(e);
            circuitBreaker.MoveToOpenState();
        }

        public override void ProtectedCodeHasBeenCalled()
        {
            base.ProtectedCodeHasBeenCalled();
            circuitBreaker.MoveToClosedState();
        }
    }
}
