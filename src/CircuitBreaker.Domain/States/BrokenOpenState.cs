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
            Update();
            return circuitBreaker;
        }

        public override CircuitBreakerState Update()
        {
            base.Update();
            if (DateTime.UtcNow >= openDateTime + base.circuitBreaker.Timeout)
            {
                return circuitBreaker.MoveToMendingState();
            }
            return this;
        }
    }
}
