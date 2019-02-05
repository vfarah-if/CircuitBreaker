﻿using System;
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
        public bool IsThresholdReached => Failures >= Threshold;

        public event EventHandler BeforeInvoke;
        public event EventHandler AfterInvoke;
        public event EventHandler Error;

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

        protected virtual void OnAfterInvoke()
        {
            var handler = AfterInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnError(Exception exception)
        {
            lastException = exception;
            Failures++;

            var handler = Error;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBeforeInvoke()
        {
            var handler = BeforeInvoke;
            handler?.Invoke(this, EventArgs.Empty);
        }

        internal CircuitBreakerState MoveToHealthyState()
        {
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState = new HealthyClosedState(this);
                return circuitBreakerState;
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        internal CircuitBreakerState MoveToBrokenState()
        {
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState = new BrokenOpenState(this);
                return circuitBreakerState;
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        internal CircuitBreakerState MoveToMendingState()
        {
            lockSlim.EnterWriteLock();
            try
            {
                circuitBreakerState = new MendingHalfState(this);
                return circuitBreakerState;
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        internal void Reset()
        {
            Failures = 0;
        }
    }
}
