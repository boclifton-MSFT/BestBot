# Python Best Practices

A concise, opinionated checklist for writing maintainable, readable, and Pythonic code. These practices are based on PEPs, community standards, and proven patterns from the Python ecosystem.

## Quick checklist

- Follow PEP 8 style guidelines; use automated formatters (black, autopep8).
- Write self-documenting code with clear, descriptive names.
- Use type hints for function signatures and complex data structures.
- Prefer list/dict comprehensions and generators for readability and performance.
- Handle exceptions explicitly; use specific exception types.
- Write docstrings for modules, classes, and public functions.
- Use virtual environments and pin dependencies with requirements files.
- Follow the principle of least surprise; be explicit rather than implicit.
- Write unit tests with meaningful assertions and good coverage.
- Use logging instead of print statements for debugging and monitoring.

---

## Code Style and Formatting

- Follow PEP 8 style guide: 79-character line limit, 4-space indentation, snake_case naming.
- Use automated formatters like `black` or `autopep8` for consistent formatting.
- Organize imports: standard library, third-party packages, local imports (separated by blank lines).
- Remove unused imports and variables; use tools like `isort` and `flake8`.

## Resources

- Python docs: https://docs.python.org/3/
- PEP 8 (style guide): https://peps.python.org/pep-0008/
- Packaging Guide: https://packaging.python.org/
- Type hints / typing docs: https://docs.python.org/3/library/typing.html
