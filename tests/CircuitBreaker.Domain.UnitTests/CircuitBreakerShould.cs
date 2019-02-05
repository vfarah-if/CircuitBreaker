using System;
using System.Threading;
using FluentAssertions;
using Xunit;
using static System.Environment;
using static CircuitBreaker.Domain.ErrorMessage;

namespace CircuitBreaker.Domain.UnitTests
{
    public class CircuitBreakerShould
    {
        private int validThreshold;
        private TimeSpan validTimeSpan;
        private CircuitBreaker circuitBreaker;
        private TestMethodCall testMethodCall;

        public CircuitBreakerShould()
        {
            validThreshold = 3;
            validTimeSpan = TimeSpan.FromMilliseconds(400);
            circuitBreaker = new CircuitBreaker(validThreshold, validTimeSpan);
            this.testMethodCall = new TestMethodCall();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowArgumentRangeExceptionWhenThresholdValueIsInvalid(int invalidThreshold)
        {
            var expectedErrorMessage = $"{ThresholdRangeInvalid}{NewLine}Parameter name: threshold" ;

            Action action = () => { new CircuitBreaker(invalidThreshold, validTimeSpan); };

            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowArgumentRangeExceptionWhenTimeoutValueIsInvalid(int invalidTimeSpan)
        {
            var expectedErrorMessage = $"{TimeoutRangeInvalid}{NewLine}Parameter name: timeout";

            Action action = () => { new CircuitBreaker(validThreshold, TimeSpan.FromSeconds(invalidTimeSpan)); };

            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage(expectedErrorMessage);
        }

        [Fact]
        public void CreateACircuitWithTheDefaultedValidState()
        {
            circuitBreaker.IsBrokenAndOpen.Should().BeFalse();
            circuitBreaker.IsMendingAndHalfway.Should().BeFalse();
            circuitBreaker.IsHealthyAndClosed.Should().BeTrue();
        }

        [Fact]
        public void CountFailuresAndStayInAHealthyClosedStateWhileTheThresholdIsNotReached()
        {
            for (var errorCount = 1; errorCount < circuitBreaker.Threshold; errorCount++)
            {
                var actual = circuitBreaker.TryInvoke(() => testMethodCall.ErrorProneMethod());
                actual.IsHealthyAndClosed.Should().BeTrue();
                actual.LastError().Should().NotBeNull();
                actual.Failures.Should().Be(errorCount);
                actual.IsThresholdReached.Should().BeFalse();
            }
        }

        [Fact]
        public void ShouldPreventCallsWhenInBrokenStateAndThenTransitionBackIntoFaultingState()
        {
            CircuitBreaker actual = null;
            for (var errorCount = 1; errorCount <= circuitBreaker.Threshold; errorCount++)
            {
                actual = circuitBreaker.TryInvoke(() => testMethodCall.ErrorProneMethod());
                
            }
            actual.IsHealthyAndClosed.Should().BeFalse();
            actual.IsBrokenAndOpen.Should().BeTrue();
            actual.Failures.Should().Be(circuitBreaker.Threshold);
            testMethodCall.FailingMethodCallCount.Should().Be(circuitBreaker.Threshold);
            actual.IsThresholdReached.Should().BeTrue();
        }

        [Fact]
        public void TransitionToBrokenStateWhenExceedingTheFailureThresholdPreventingAnyCallsInTheTimeoutPeriod()
        {
            CircuitBreaker actual = null;
            for (var errorCount = 1; errorCount <= circuitBreaker.Threshold; errorCount++)
            {
                actual = circuitBreaker.TryInvoke(() => testMethodCall.ErrorProneMethod());
            }

            actual.Should().NotBeNull();
            circuitBreaker.TryInvoke(() => testMethodCall.SuccessProneMethod());
            
        }

        [Fact]
        public void TransitionBrokenStateAfterTimeOutPeriodToHealthyHalfStateOnASuccessfulSingleCall()
        {
            CircuitBreaker actual = null;
            for (var errorCount = 1; errorCount <= circuitBreaker.Threshold; errorCount++)
            {
                actual = circuitBreaker.TryInvoke(() => testMethodCall.ErrorProneMethod());
            }
           
            Thread.Sleep(validTimeSpan);

            actual.Should().NotBeNull();
            var beforeInvokeCallCount = 0;
            actual.BeforeInvoke += (sender, args) =>
            {
                actual.IsMendingAndHalfway.Should().BeTrue();
                beforeInvokeCallCount++;
            };
            actual = actual.TryInvoke(() => testMethodCall.SuccessProneMethod());
            testMethodCall.SuccessMethodCallCount.Should().Be(1);
            beforeInvokeCallCount.Should().Be(1);
            actual.IsMendingAndHalfway.Should().BeFalse();
            actual.IsHealthyAndClosed.Should().BeTrue();
            actual.IsThresholdReached.Should().BeFalse();
            actual.Failures.Should().Be(0);
        }

        [Fact]
        public void TransitionBrokenStateAfterTimeOutPeriodBackToABrokenStateOnAFailedSingleCall()
        {
            CircuitBreaker actual = null;
            for (var errorCount = 1; errorCount <= circuitBreaker.Threshold; errorCount++)
            {
                actual = circuitBreaker.TryInvoke(() => testMethodCall.ErrorProneMethod());
            }

            Thread.Sleep(validTimeSpan);

            actual.Should().NotBeNull();
            var beforeInvokeCallCount = 0;
            actual.BeforeInvoke += (sender, args) =>
            {
                actual.IsMendingAndHalfway.Should().BeTrue();
                beforeInvokeCallCount++;
            };
            actual = actual.TryInvoke(() => testMethodCall.ErrorProneMethod());

            testMethodCall.FailingMethodCallCount.Should().Be(4);
            actual.Failures.Should().Be(4);
            beforeInvokeCallCount.Should().Be(1);
            actual.IsMendingAndHalfway.Should().BeFalse();
            actual.IsHealthyAndClosed.Should().BeFalse();
            actual.IsBrokenAndOpen.Should().BeTrue();
            actual.IsThresholdReached.Should().BeTrue();
        }
    }
}
