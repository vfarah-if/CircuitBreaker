using System;

namespace CircuitBreaker.Domain
{
    public class MendingHalfState : CircuitBreakerState
    {
        public MendingHalfState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }

        internal override void OnError(Exception e)
        {
            circuitBreaker.MoveToBrokenState();
        }

        internal override void OnAfterInvoke()
        {
            circuitBreaker.MoveToHealthyState();
        }
    }
}
