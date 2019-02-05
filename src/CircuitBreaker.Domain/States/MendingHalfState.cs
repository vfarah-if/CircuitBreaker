using System;

namespace CircuitBreaker.Domain
{
    public class MendingHalfState : CircuitBreakerState
    {
        public MendingHalfState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }

        internal override void OnError(Exception e)
        {
            base.OnError(e);
            circuitBreaker.MoveToBrokenState();
        }

        internal override void OnAfterInvoke()
        {
            base.OnAfterInvoke();
            circuitBreaker.MoveToHealthyState();
        }
    }
}
