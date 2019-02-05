using System;
using static CircuitBreaker.Domain.ErrorMessage;

namespace CircuitBreaker.Domain
{
    public class CircuitBreaker
    {
        private volatile object syncLock = new object();
        private CircuitBreakerState state;
        private Exception lastException;

        public CircuitBreaker(int threshold, TimeSpan timeout)
        {
            if (threshold <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), ThresholdRangeInvalid);
            }

            if (timeout.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), TimeoutRangeInvalid);
            }

            Threshold = threshold;
            Timeout = timeout;
            MoveToHealthyState();
        }

        public int Failures { get; private set; }
        public int Threshold { get; }
        public TimeSpan Timeout { get; }
        public bool IsHealthyAndClosed => state.Update() is HealthyClosedState;
        public bool IsBrokenAndOpen => state.Update() is BrokenOpenState;
        public bool IsMendingAndHalfway => state.Update() is MendingHalfState;

        public event EventHandler BeforeInvoke;
        public event EventHandler AfterInvoke;
        public event EventHandler Error;

        public bool IsThresholdReached()
        {
            return Failures >= Threshold;
        }

        public Exception LastError()
        {
            return lastException;
        }

        public CircuitBreaker TryInvoke(Action action)
        {
            lastException = null;
            lock (syncLock)
            {
                OnBeforeInvoke();
                if (state is BrokenOpenState)
                {
                    return this; 
                }
            }

            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                OnError(e);               
                return this; 
            }

            OnAfterInvoke();
            return this;
        }

        private void OnAfterInvoke()
        {
            lock (syncLock)
            {
                state.OnAfterInvoke();
            }
            var handler = AfterInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnError(Exception exception)
        {
            lastException = exception;
            lock (syncLock)
            {
                state.OnError(exception);
            }
            var handler = Error;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnBeforeInvoke()
        {
            state.OnBeforeInvoke();
            var handler = BeforeInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        internal CircuitBreakerState MoveToHealthyState()
        {
            state = new HealthyClosedState(this);
            return state;
        }

        internal CircuitBreakerState MoveToBrokenState()
        {
            state = new BrokenOpenState(this);
            return state;
        }

        internal CircuitBreakerState MoveToMendingState()
        {
            state = new MendingHalfState(this);
            return state;
        }

        internal void IncreaseFailureCount()
        {
            Failures++;
        }

        internal void ResetFailureCount()
        {
            Failures = 0;
        }
    }
}
