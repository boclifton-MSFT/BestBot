# Ruby Best Practices

A concise, opinionated checklist for writing maintainable, readable, and idiomatic Ruby code. These practices are based on Ruby Style Guide, community standards, and proven patterns from the Ruby ecosystem.

## Quick checklist

- Follow Ruby Style Guide conventions; use automated formatters (RuboCop).
- Write self-documenting code with clear, descriptive method and variable names.
- Prefer blocks and iterators over traditional loops for better readability.
- Use meaningful method names that express intent; avoid abbreviations.
- Handle exceptions explicitly; prefer specific rescue clauses.
- Write comprehensive tests using RSpec or Minitest.
- Use bundler for dependency management and lock versions.
- Follow the principle of least surprise; be explicit and clear.
- Prefer symbols over strings for hash keys and constants.
- Use duck typing and embrace Ruby's dynamic nature responsibly.

---

## Code Style and Formatting

- Follow the Ruby Style Guide: 2-space indentation, snake_case for methods/variables, PascalCase for classes/modules.
- Use RuboCop for automated style checking and formatting.
- Keep line length under 120 characters; prefer 80 for readability.
- Use meaningful whitespace to separate logical sections of code.
- Prefer single quotes for strings unless interpolation is needed.

## Naming and Structure

- Use descriptive method names that read like natural language (`user.authenticate!` vs `user.auth`).
- Choose intention-revealing names: `users.select(&:active?)` vs `users.select { |u| u.status == 1 }`.
- Use snake_case for file names, method names, and variables.
- Use PascalCase for classes and modules, SCREAMING_SNAKE_CASE for constants.
- Organize code into modules and classes with single responsibilities.

## Idiomatic Ruby Patterns

- Prefer blocks and iterators: `users.each { |user| user.activate! }` over traditional for loops.
- Use safe navigation operator when appropriate: `user&.profile&.name`.
- Leverage Ruby's expressive conditional statements: `return unless user.valid?`.
- Use splat operators for flexible method signatures: `def process(*args, **kwargs)`.
- Prefer string interpolation over concatenation: `"Hello, #{name}!"` vs `"Hello, " + name + "!"`.

## Object-Oriented Design

- Follow SOLID principles; keep classes focused on single responsibilities.
- Use modules for shared behavior and include/extend appropriately.
- Prefer composition over inheritance for complex relationships.
- Use attr_accessor, attr_reader, attr_writer instead of manual getter/setter methods.
- Implement `to_s` and `inspect` methods for custom classes to aid debugging.

## Error Handling and Exceptions

- Use specific exception types rather than generic RuntimeError.
- Prefer `rescue` with specific exception classes: `rescue ActiveRecord::RecordNotFound`.
- Use `ensure` blocks for cleanup code that must run regardless of exceptions.
- Raise exceptions early when preconditions are not met.
- Consider using `fail` for unrecoverable errors and `raise` for recoverable ones.

## Testing and Quality

- Write tests first (TDD) or alongside code development.
- Use descriptive test names that explain behavior: `it "authenticates user with valid credentials"`.
- Prefer RSpec for behavior-driven testing or Minitest for simpler unit tests.
- Use factories (FactoryBot) or fixtures for test data setup.
- Mock external dependencies and focus on testing your code's behavior.
- Aim for high test coverage but focus on critical business logic.

## Performance and Resource Management

- Use symbols for hash keys when possible for better memory efficiency.
- Prefer `String#freeze` for immutable strings in performance-critical code.
- Use lazy evaluation with `Enumerator::Lazy` for large datasets.
- Profile before optimizing; use tools like `ruby-prof` or `benchmark-ips`.
- Be mindful of memory allocation in tight loops.

## Dependencies and Bundler

- Use `Gemfile` and `Gemfile.lock` for dependency management.
- Specify version constraints thoughtfully: `gem 'rails', '~> 7.0'`.
- Group gems appropriately (development, test, production).
- Regularly update dependencies and monitor for security vulnerabilities.
- Use `bundle exec` to ensure correct gem versions in scripts.

## Security Considerations

- Validate and sanitize all user input.
- Use strong parameters in Rails applications.
- Be cautious with `eval`, `instance_eval`, and dynamic method calls.
- Use secure random generators: `SecureRandom.hex(16)` instead of `rand`.
- Keep gems updated to patch security vulnerabilities.

## Documentation and Comments

- Write self-documenting code with clear method and variable names.
- Use YARD documentation format for public APIs.
- Comment complex business logic and non-obvious algorithmic decisions.
- Keep README files updated with setup and usage instructions.
- Use inline comments sparingly; prefer extracting complex logic into well-named methods.

## Tooling and Development Environment

- Use a consistent Ruby version manager (rbenv, RVM, or asdf).
- Configure your editor with Ruby language support and linting.
- Use Guard or similar tools for automated testing during development.
- Set up continuous integration to run tests and style checks.
- Use debugging tools like `pry` or `byebug` instead of `puts` debugging.

## Rails-Specific Best Practices (if applicable)

- Follow Rails conventions: "Convention over Configuration".
- Use strong parameters to whitelist permitted attributes.
- Keep controllers thin; move business logic to models or service objects.
- Use scopes in models for reusable query logic.
- Leverage Rails' built-in helpers and avoid reinventing common functionality.
- Use database constraints and validations appropriately.

Minimal Fallback (if file unavailable)
- Write clear, expressive code that reads like natural language
- Use blocks and iterators for better readability
- Follow Ruby Style Guide conventions
- Test your code thoroughly
- Manage dependencies with Bundler