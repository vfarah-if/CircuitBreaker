using System;

namespace CircuitBreaker.Domain
{
    public class MendingHalfState : CircuitBreakerState
    {
        public MendingHalfState(CircuitBreaker circuitBreaker) : base(circuitBreaker)
        {
            circuitBreaker.Error += OnError;
            circuitBreaker.AfterInvoke += OnAfterInvoke;
        }

        private void OnAfterInvoke(object sender, EventArgs e)
        {
            circuitBreaker.MoveToHealthyState();
        }

        private void OnError(object sender, EventArgs e)
        {
            circuitBreaker.MoveToBrokenState();
        }
    }
}