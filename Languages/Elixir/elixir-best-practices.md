---
language_version: "1.18"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://elixir-lang.org/downloads.html"
---

# Elixir Best Practices

A comprehensive guide to writing maintainable, functional, and fault-tolerant Elixir applications based on the official Elixir documentation, OTP design principles, and community standards.

## Overview

Elixir is a dynamic, functional programming language built on the Erlang VM (BEAM). It combines Erlang's battle-tested concurrency model and fault-tolerance with a modern, Ruby-inspired syntax and powerful metaprogramming via macros. Elixir's key strengths are lightweight processes, supervision trees, hot code upgrades, and the pipe operator for clear data transformation pipelines. It excels in building concurrent, distributed, and fault-tolerant systems that run with low latency at scale.

## When to use Elixir in projects

- **Real-time web applications**: Phoenix LiveView for interactive UIs without JavaScript
- **APIs and microservices**: Phoenix framework with JSON or GraphQL endpoints
- **Chat and messaging**: High-concurrency connection handling (WhatsApp-style)
- **IoT and embedded**: Nerves framework for embedded systems
- **Data pipelines**: Broadway and GenStage for backpressure-aware data processing
- **Distributed systems**: Multi-node clustering with built-in distribution
- **Event-driven architectures**: Process-based event sourcing and CQRS

## Tooling & ecosystem

### Core tools
- **Runtime**: BEAM (Erlang VM) with OTP
- **Build tool**: [Mix](https://hexdocs.pm/mix/) (built-in project management, tasks, dependencies)
- **Package manager**: [Hex](https://hex.pm/) with `mix.exs` and `mix.lock`
- **Formatter**: `mix format` (built-in, opinionated)
- **Documentation**: [ExDoc](https://hexdocs.pm/ex_doc/) with `@doc` and `@moduledoc`

### Project setup

```bash
mix new my_app
cd my_app
mix deps.get
mix compile
```

## Recommended formatting & linters

### mix format (built-in, recommended)

```bash
# Format all project files
mix format

# Check formatting in CI
mix format --check-formatted
```

Configure in `.formatter.exs`:

```elixir
[
  inputs: ["{mix,.formatter}.exs", "{config,lib,test}/**/*.{ex,exs}"],
  line_length: 98
]
```

### Code style essentials

- `snake_case` for functions, variables, and file names; `PascalCase` for modules
- Use the pipe operator (`|>`) for data transformation chains
- Prefer pattern matching over conditional statements
- Use guard clauses for type constraints on function heads
- Organize module attributes: `alias`, `import`, `use` at the top

```elixir
defmodule MyApp.Greeter do
  @moduledoc "Greeting functions."

  @spec greet(String.t()) :: String.t()
  def greet(name) when is_binary(name), do: "Hello, #{name}!"
  def greet(_), do: {:error, :invalid_name}
end
```

## Testing & CI recommendations

### ExUnit (built-in)

```bash
mix test
mix test --cover
```

Example test with doctests:

```elixir
defmodule MyApp.GreeterTest do
  use ExUnit.Case, async: true
  doctest MyApp.Greeter

  describe "greet/1" do
    test "returns greeting for valid name" do
      assert MyApp.Greeter.greet("Elixir") == "Hello, Elixir!"
    end

    test "returns error for non-string input" do
      assert MyApp.Greeter.greet(42) == {:error, :invalid_name}
    end
  end
end
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: erlef/setup-beam@v1
        with:
          otp-version: 27
          elixir-version: 1.17
      - run: mix deps.get
      - run: mix format --check-formatted
      - run: mix compile --warnings-as-errors
      - run: mix test --cover
```

## Packaging & release guidance

- Publish libraries to Hex with `mix hex.publish`
- Use `mix release` for self-contained production releases (OTP releases)
- Document all public modules and functions with `@doc` and `@moduledoc`
- Follow semantic versioning; maintain a CHANGELOG
- Use `@spec` type specifications for all public functions

## Security & secrets best practices

- Store secrets in environment variables; use `config/runtime.exs` for runtime config
- Validate and sanitize all external inputs at application boundaries
- Use Ecto changesets for data validation and type casting
- Avoid string interpolation with user input in queries; use parameterized queries
- Keep dependencies updated; run `mix hex.audit` to scan for vulnerabilities
- Never log sensitive data (passwords, tokens, PII)

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Web framework | [Phoenix](https://www.phoenixframework.org/) | Full-stack with LiveView |
| Database | [Ecto](https://hexdocs.pm/ecto/) | Database wrapper and query DSL |
| HTTP client | [Req](https://hexdocs.pm/req/) / [Tesla](https://hexdocs.pm/tesla/) | Modern / middleware-based |
| Background jobs | [Oban](https://hexdocs.pm/oban/) | Postgres-backed job processing |
| Testing | [ExUnit](https://hexdocs.pm/ex_unit/) (built-in) | Concurrent test framework |
| Data pipelines | [Broadway](https://hexdocs.pm/broadway/) | Multi-stage data ingestion |

## Minimal example

```elixir
# hello.exs
defmodule Hello do
  def greet(name \\ "world"), do: "Hello, #{name}!"
end

IO.puts(Hello.greet("Elixir"))
```

```bash
elixir hello.exs
# Output: Hello, Elixir!
```

## Further reading

- [Elixir Getting Started Guide](https://elixir-lang.org/getting-started/introduction.html) — official language tutorial
- [Programming Elixir by Dave Thomas](https://pragprog.com/titles/elixir16/programming-elixir-1-6/) — comprehensive language guide
- [OTP Design Principles](https://erlang.org/doc/design_principles/des_princ.html) — supervision, GenServer, and fault tolerance

## Resources

- Elixir official documentation — https://elixir-lang.org/docs.html
- HexDocs (package documentation) — https://hexdocs.pm/
- Hex package registry — https://hex.pm/
- Phoenix framework — https://www.phoenixframework.org/
- Elixir Style Guide — https://github.com/christopheradams/elixir_style_guide
- OTP Design Principles — https://erlang.org/doc/design_principles/des_princ.html
- ExUnit testing docs — https://hexdocs.pm/ex_unit/
- Elixir Forum (community) — https://elixirforum.com/
