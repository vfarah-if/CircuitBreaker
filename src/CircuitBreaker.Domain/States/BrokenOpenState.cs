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
        }

        internal override CircuitBreaker OnBeforeInvoke()
        {
            base.OnBeforeInvoke();
            GetState();
            return circuitBreaker;
        }

        public override CircuitBreakerState GetState()
        {
            base.GetState();
            return DateTime.UtcNow >= openDateTime + base.circuitBreaker.Timeout ? circuitBreaker.MoveToMendingState() : this;
        }
    }
}
