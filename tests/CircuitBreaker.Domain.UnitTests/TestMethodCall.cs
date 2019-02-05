using System;

namespace CircuitBreaker.Domain.UnitTests
{
    public class TestMethodCall
    {
        public TestMethodCall()
        {
            FailingMethodCallCount = 0;
            SuccessMethodCallCount = 0;
        }

        public int FailingMethodCallCount { get; private set; }
        public int SuccessMethodCallCount { get; private set; }

        public void ErrorProneMethod()
        {
            FailingMethodCallCount++;
            throw new Exception("Failed with error");
        }

        public void SuccessProneMethod()
        {
            SuccessMethodCallCount++;
        }
    }
}
