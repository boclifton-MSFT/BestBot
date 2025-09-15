# PHP Best Practices

A comprehensive guide to writing clean, secure, and maintainable PHP code. These practices are based on PHP-FIG standards, community recommendations, and modern PHP development patterns.

## Quick Checklist

- Follow PSR-1, PSR-2, and PSR-12 coding standards for consistent formatting and style.
- Use strict types by declaring `declare(strict_types=1);` at the top of files.
- Validate and sanitize all user input; never trust external data.
- Use type declarations for function parameters and return values.
- Prefer dependency injection over global state and static methods.
- Write comprehensive unit tests with PHPUnit or similar frameworks.
- Use Composer for dependency management and autoloading.
- Implement proper error handling with exceptions rather than error codes.
- Use prepared statements for database queries to prevent SQL injection.
- Keep functions and classes small and focused on a single responsibility.

---

## Code Style and Standards

- Follow PSR-1 (Basic Coding Standard) and PSR-12 (Extended Coding Style) for formatting.
- Use 4 spaces for indentation, not tabs.
- Keep line length under 120 characters when possible.
- Use camelCase for variables and methods, PascalCase for classes.
- Place opening braces on the same line for methods and control structures.
- Use meaningful names for variables, functions, and classes.

## Type Safety and Modern PHP

- Always use `declare(strict_types=1);` at the beginning of PHP files.
- Declare parameter types and return types for all functions and methods.
- Use nullable types (`?string`) when appropriate.
- Prefer union types (`string|int`) over mixed types in PHP 8+.
- Use readonly properties for immutable data in PHP 8.1+.
- Leverage enums for fixed sets of values in PHP 8.1+.

## Security Best Practices

- Validate and sanitize all input data using filter functions or validation libraries.
- Use prepared statements for database queries; never concatenate user input into SQL.
- Escape output with `htmlspecialchars()` or similar functions to prevent XSS.
- Use CSRF tokens for state-changing operations.
- Store sensitive configuration in environment variables, not in code.
- Use password_hash() and password_verify() for password handling.
- Implement proper session security (secure cookies, regeneration, etc.).

## Database and SQL

- Always use prepared statements with PDO or MySQLi.
- Use transactions for multi-step database operations.
- Implement proper connection pooling and error handling.
- Avoid exposing database structure in error messages.
- Use database migrations for schema changes.
- Index frequently queried columns appropriately.

## Error Handling and Logging

- Use exceptions for error conditions rather than return codes.
- Create custom exception classes for domain-specific errors.
- Log errors appropriately but avoid logging sensitive information.
- Use try-catch blocks judiciously; don't catch exceptions you can't handle.
- Implement global exception handlers for unhandled exceptions.
- Use PSR-3 compatible logging libraries like Monolog.

## Object-Oriented Programming

- Follow SOLID principles for class design.
- Use dependency injection containers for managing dependencies.
- Prefer composition over inheritance.
- Make classes and methods as small and focused as possible.
- Use interfaces to define contracts and enable polymorphism.
- Implement the Single Responsibility Principle for classes and methods.

## Performance and Optimization

- Use OPcache in production environments.
- Profile your application to identify bottlenecks before optimizing.
- Cache expensive operations appropriately (Redis, Memcached, etc.).
- Use lazy loading for heavy resources.
- Optimize database queries and use appropriate indexes.
- Consider using PHP 8+ features like JIT compilation when beneficial.

## Testing and Quality Assurance

- Write unit tests with PHPUnit for critical business logic.
- Use integration tests for testing component interactions.
- Implement code coverage reporting and aim for meaningful coverage.
- Use static analysis tools like PHPStan or Psalm for code quality.
- Run automated tests in CI/CD pipelines.
- Use tools like PHP_CodeSniffer for coding standard enforcement.

## Dependency Management

- Use Composer for managing dependencies and autoloading.
- Keep composer.json and composer.lock in version control.
- Use semantic versioning constraints appropriately.
- Regularly update dependencies but test thoroughly.
- Use separate dev and production dependency groups.
- Consider using private Packagist for proprietary packages.

## Configuration and Environment

- Use environment variables for configuration that changes between environments.
- Never commit sensitive data like passwords or API keys to version control.
- Use configuration libraries like vlucas/phpdotenv for environment management.
- Validate configuration values at application startup.
- Use separate configuration files for different environments.

## Documentation and Comments

- Write clear, concise docblock comments for classes and public methods.
- Use PHPDoc standards for documenting parameters, return types, and exceptions.
- Keep comments up-to-date with code changes.
- Document complex algorithms and business logic.
- Avoid obvious comments that just restate the code.
- Consider using documentation generators like phpDocumentor.

Minimal Fallback (if file unavailable)
- Follow PSR standards for consistent code style
- Use strict types and type declarations
- Validate all input and escape all output
- Use prepared statements for database queries
- Write comprehensive tests and use static analysis tools