using System;
using System.Threading;
using static CircuitBreaker.Domain.ErrorMessage;

namespace CircuitBreaker.Domain
{
    public class CircuitBreaker
    {
        private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
        private CircuitBreakerState circuitBreakerState;
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
        public bool IsHealthyAndClosed => circuitBreakerState is HealthyClosedState;
        public bool IsBrokenAndOpen => circuitBreakerState is BrokenOpenState;
        public bool IsMendingAndHalfway => circuitBreakerState is MendingHalfState;

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
          
            OnBeforeInvoke();
            if (circuitBreakerState is BrokenOpenState)
            {
                return this; 
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
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState.OnAfterInvoke();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
            var handler = AfterInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnError(Exception exception)
        {
            lastException = exception;
            Failures++;
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState.OnError(exception);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
            var handler = Error;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnBeforeInvoke()
        {
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState.OnBeforeInvoke();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }

            var handler = BeforeInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        internal CircuitBreakerState MoveToHealthyState()
        {
            circuitBreakerState = new HealthyClosedState(this);
            return circuitBreakerState;
        }

        internal CircuitBreakerState MoveToBrokenState()
        {
            circuitBreakerState = new BrokenOpenState(this);
            return circuitBreakerState;
        }

        internal CircuitBreakerState MoveToMendingState()
        {
            circuitBreakerState = new MendingHalfState(this);
            return circuitBreakerState;
        }

        internal void Reset()
        {
            Failures = 0;
        }
    }
}
