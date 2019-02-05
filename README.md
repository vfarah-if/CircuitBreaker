# CircuitBreaker
Analysis utilising ideas from https://en.wikipedia.org/wiki/Circuit_breaker_design_pattern

## Story - Implement a circuit breaker

    As a systems maintainer
    I want my calls to be resilient to failure
    So I don't need to turn features on and off

        - Implement the circuit breaker pattern
        - Fault tolerance should be configurable
        - Half-open or Healing timeouts should be configurable


### Scenario 1 - Monitoring failure

    As a developer
    I want to count failures when I make calls
    A failure count is incremented

### Scenario 2 - Opening a circuit

    As a developer
    When my failure count exceeds a configured tolerance
    All subsequent calls throw an exception and block the call

### Scenario 3 -  Half opened state

    As a developer
    When my code is blocking calls and a configured timeout passes
    I allow a single call to proceed

        - Single call succeeding closes circuit
        - Single call failing resets some kind of timeout

# Summary
Final conclusion, [Polly](https://github.com/App-vNext/Polly) is still the most appropriate library to use when trying several types of resilience methods on a call, including the circuit breaker with possibilities to do manual circuit