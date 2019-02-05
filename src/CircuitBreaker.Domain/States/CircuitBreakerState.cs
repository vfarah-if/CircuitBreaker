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

        public virtual CircuitBreakerState Update()
        {
            return this;
        }

        internal virtual CircuitBreaker OnBeforeInvoke()
        {
            return circuitBreaker;
        }

        internal virtual void OnAfterInvoke()
        {
        }

        internal virtual void OnError(Exception e)
        {
            circuitBreaker.IncreaseFailureCount();
        }
    }
}
