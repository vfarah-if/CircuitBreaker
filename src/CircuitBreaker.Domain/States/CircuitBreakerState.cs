using System;

namespace CircuitBreaker.Domain
{
    public abstract class CircuitBreakerState
    {
        protected readonly CircuitBreaker circuitBreaker;

        protected CircuitBreakerState(CircuitBreaker circuitBreaker)
        {
            this.circuitBreaker = circuitBreaker;
        }

        internal virtual void OnBeforeInvoke()
        {
        }

        internal virtual void OnAfterInvoke()
        {
        }

        internal virtual void OnError(Exception e)
        {
        }
    }
}
