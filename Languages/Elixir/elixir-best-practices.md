# Elixir Best Practices

A concise, opinionated checklist for writing maintainable, functional, and fault-tolerant Elixir code. These practices are based on the official Elixir documentation, OTP design principles, and community standards.

## Quick checklist

- Follow Elixir's naming conventions: snake_case for variables/functions, PascalCase for modules.
- Use pattern matching instead of conditional statements when possible.
- Embrace immutability; avoid mutating data structures.
- Design for fault tolerance with supervision trees and "let it crash" philosophy.
- Use GenServer and other OTP behaviors for stateful processes.
- Write comprehensive doctests alongside regular tests.
- Use pipe operator (|>) for data transformation chains.
- Prefer small, focused functions that do one thing well.
- Handle errors explicitly with {:ok, result} and {:error, reason} tuples.
- Use async/await patterns with Task module for concurrent operations.

---

## Code Style and Formatting

- Use `mix format` for consistent code formatting and enforce it in CI.
- Follow the 98-character line limit (configurable in `.formatter.exs`).
- Use snake_case for variable names, function names, and file names.
- Use PascalCase for module names and atoms representing module names.
- Prefer explicit parentheses for function calls with multiple arguments.
- Organize imports with alias, import, and use at the top of modules.

## Project Structure and Organization

- Follow the standard Mix project layout: `lib/`, `test/`, `config/`, `priv/`.
- Group related modules in subdirectories under `lib/`.
- Use one module per file, with the file path matching the module name.
- Keep application configuration in `config/` files (config.exs, dev.exs, prod.exs).
- Place assets and non-code resources in `priv/`.

## Pattern Matching and Functions

- Leverage pattern matching in function heads for control flow.
- Use guard clauses to add constraints to pattern matches.
- Define multiple function clauses for different input patterns.
- Prefer pattern matching over conditional statements (if/case).
- Use the match operator (=) to destructure and assert on data shapes.

```elixir
# Good: Pattern matching in function heads
def process({:ok, data}), do: transform(data)
def process({:error, reason}), do: handle_error(reason)

# Good: Pattern matching with guards
def calculate(x) when is_number(x) and x > 0, do: x * 2
def calculate(_), do: {:error, :invalid_input}
```

## Error Handling and Fault Tolerance

- Use tagged tuples {:ok, result} and {:error, reason} for explicit error handling.
- Embrace the "let it crash" philosophy for unexpected errors.
- Design supervision trees to restart failed processes.
- Use `with` statements for happy path error handling chains.
- Prefer `!` functions (like `File.read!`) only when you're certain of success.

```elixir
# Good: Explicit error handling
def safe_divide(a, b) do
  case b do
    0 -> {:error, :division_by_zero}
    _ -> {:ok, a / b}
  end
end

# Good: Using 'with' for chaining operations
def process_user(id) do
  with {:ok, user} <- fetch_user(id),
       {:ok, profile} <- fetch_profile(user),
       {:ok, result} <- transform_data(profile) do
    {:ok, result}
  end
end
```

## OTP and Process Design

- Use GenServer for stateful processes that need lifecycle management.
- Implement proper supervision strategies (one_for_one, rest_for_one, one_for_all).
- Design processes to be lightweight and focused on single responsibilities.
- Use process links and monitors for process communication and health monitoring.
- Prefer message passing over shared state for process communication.

## Data Transformation and Pipelines

- Use the pipe operator (|>) for data transformation chains.
- Prefer Enum functions over explicit recursion for list processing.
- Use Stream for lazy evaluation of large datasets.
- Leverage Elixir's immutable data structures effectively.

```elixir
# Good: Pipeline for data transformation
users
|> Enum.filter(&active?/1)
|> Enum.map(&format_user/1)
|> Enum.sort_by(& &1.name)
```

## Testing and Documentation

- Write doctests for functions to provide both tests and examples.
- Use ExUnit for comprehensive test suites with clear test descriptions.
- Test both success and failure scenarios for each function.
- Use `setup` and `setup_all` for test data preparation.
- Document modules and functions with `@doc` and `@moduledoc`.

```elixir
@doc """
Calculates the area of a circle given its radius.

## Examples

    iex> Circle.area(5)
    78.53981633974483

    iex> Circle.area(0)
    0
"""
def area(radius), do: :math.pi() * radius * radius
```

## Performance and Optimization

- Profile before optimizing; use tools like `:observer` and `mix profile.eprof`.
- Prefer tail recursion for recursive functions to avoid stack overflow.
- Use binary pattern matching for efficient string processing.
- Consider using ETS or Agent for in-memory state when appropriate.
- Leverage BEAM's concurrent processing capabilities with Task and GenStage.

## Dependencies and Configuration

- Use `mix deps.get` and lock versions in `mix.lock` for reproducible builds.
- Keep dependencies minimal and prefer established, well-maintained libraries.
- Use configuration for environment-specific settings, not application logic.
- Prefer compile-time configuration when possible for better performance.

## Concurrency and Distribution

- Use Task for fire-and-forget concurrent operations.
- Implement backpressure with GenStage for data processing pipelines.
- Use Registry for process discovery and PubSub for event distribution.
- Design for horizontal scaling with distributed Erlang when needed.

## Security and Input Validation

- Validate and sanitize all external inputs at application boundaries.
- Use Ecto changesets for data validation and type casting.
- Avoid string interpolation with user input; use parameterized queries.
- Implement proper authentication and authorization patterns.

## Common Anti-patterns to Avoid

- Don't use processes for computational tasks; processes are for state and fault tolerance.
- Avoid deep nesting of case statements; use function clauses instead.
- Don't ignore function return values, especially error tuples.
- Avoid large GenServer state; consider breaking into smaller processes.
- Don't use `throw/catch` for normal control flow; reserve for exceptional cases.

## Resources

- Elixir Official Documentation: https://elixir-lang.org/docs.html
- HexDocs: https://hexdocs.pm/
- Elixir Style Guide: https://github.com/christopheradams/elixir_style_guide
- OTP Design Principles: https://erlang.org/doc/design_principles/des_princ.html

---

Minimal Fallback (if file unavailable)
- Use pattern matching and immutable data
- Design with supervision trees and fault tolerance
- Write doctests and comprehensive tests
- Follow OTP conventions and behaviors