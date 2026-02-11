# Python Best Practices

A comprehensive guide to writing maintainable, readable, and Pythonic code based on PEPs, community standards, and proven patterns from the Python ecosystem.

## Overview

Python is a high-level, dynamically typed, general-purpose programming language that emphasizes readability and simplicity. Its design philosophy, captured in "The Zen of Python" (`import this`), prioritizes explicit over implicit, simple over complex, and readability as a core value. Python's extensive standard library, mature ecosystem, and gentle learning curve make it one of the most widely used languages across web development, data science, automation, and systems scripting.

## When to use Python in projects

- **Web applications and APIs**: Django, Flask, FastAPI for rapid backend development
- **Data science and machine learning**: NumPy, pandas, scikit-learn, PyTorch, TensorFlow
- **Scripting and automation**: System administration, file processing, task automation
- **Scientific computing**: Research, simulations, data analysis pipelines
- **DevOps and infrastructure**: Ansible, configuration management, CLI tools
- **Prototyping**: Rapid iteration and proof-of-concept development
- **ETL and data pipelines**: Data extraction, transformation, and loading workflows

## Tooling & ecosystem

### Core tools
- **Interpreter**: CPython (reference), PyPy (JIT-compiled)
- **Package manager**: `pip` with `pyproject.toml` or `requirements.txt`; `uv` for fast dependency resolution
- **Virtual environments**: `venv` (built-in), `virtualenv`, `conda`
- **Formatter**: [Black](https://github.com/psf/black) (opinionated), [Ruff](https://github.com/astral-sh/ruff) (fast, includes formatting)
- **Linter**: [Ruff](https://github.com/astral-sh/ruff), [Flake8](https://flake8.pycqa.org/), [Pylint](https://pylint.readthedocs.io/)
- **Type checker**: [mypy](https://mypy.readthedocs.io/), [Pyright](https://github.com/microsoft/pyright)

### Package management

```bash
# Create a virtual environment
python -m venv .venv
source .venv/bin/activate  # Linux/macOS
.venv\Scripts\activate     # Windows

# Install dependencies
pip install -r requirements.txt

# Modern project setup with pyproject.toml
pip install -e ".[dev]"
```

## Recommended formatting & linters

### Ruff (recommended all-in-one)

```bash
pip install ruff
ruff check src/       # Lint
ruff format src/      # Format
```

Example `pyproject.toml` configuration:

```toml
[tool.ruff]
line-length = 88
target-version = "py312"

[tool.ruff.lint]
select = ["E", "F", "I", "N", "W", "UP", "S", "B"]

[tool.ruff.format]
quote-style = "double"
```

### Type checking with mypy

```bash
pip install mypy
mypy src/ --strict
```

### Code style essentials (PEP 8)

- 4-space indentation, 79-character line limit (88 with Black/Ruff)
- `snake_case` for functions/variables, `PascalCase` for classes, `UPPER_SNAKE_CASE` for constants
- Organize imports: standard library, third-party, local (use `isort` or Ruff's `I` rules)
- Use type hints for function signatures and complex data structures

```python
from collections.abc import Sequence

def calculate_average(values: Sequence[float]) -> float:
    """Calculate the arithmetic mean of a sequence of numbers."""
    if not values:
        raise ValueError("Cannot calculate average of empty sequence")
    return sum(values) / len(values)
```

## Testing & CI recommendations

### pytest (standard test framework)

```bash
pip install pytest pytest-cov
pytest tests/ -v --cov=src --cov-report=term-missing
```

Example test (`tests/test_calculator.py`):

```python
import pytest
from myapp.calculator import calculate_average

def test_average_of_numbers():
    assert calculate_average([1.0, 2.0, 3.0]) == 2.0

def test_average_empty_raises():
    with pytest.raises(ValueError):
        calculate_average([])
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        python-version: ["3.11", "3.12", "3.13"]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: ${{ matrix.python-version }}
      - run: pip install -e ".[dev]"
      - run: ruff check src/
      - run: ruff format --check src/
      - run: mypy src/ --strict
      - run: pytest tests/ --cov=src
```

## Packaging & release guidance

- Use `pyproject.toml` as the single source of project metadata (PEP 621)
- Build with `python -m build` and upload with `twine upload dist/*`
- Use semantic versioning and maintain a changelog
- Include `py.typed` marker for PEP 561 typed packages

```toml
[project]
name = "my-package"
version = "1.0.0"
requires-python = ">=3.11"
dependencies = ["requests>=2.28"]

[project.optional-dependencies]
dev = ["pytest", "ruff", "mypy"]
```

## Security & secrets best practices

- Never commit secrets to source control; use environment variables or `.env` files with `python-dotenv`
- Validate and sanitize all external input; use parameterized queries for SQL
- Use `secrets` module for cryptographic random values, not `random`
- Run `pip audit` or `safety check` to scan dependencies for known vulnerabilities
- Pin dependency versions in production and review updates before upgrading
- Avoid `eval()`, `exec()`, and `pickle.loads()` with untrusted input

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| HTTP client | [requests](https://docs.python-requests.org/) / [httpx](https://www.python-httpx.org/) | Sync / async HTTP |
| Web framework | [FastAPI](https://fastapi.tiangolo.com/) / [Django](https://www.djangoproject.com/) | Modern async / full-stack |
| Data processing | [pandas](https://pandas.pydata.org/) | DataFrames and analysis |
| Testing | [pytest](https://docs.pytest.org/) | De facto test framework |
| CLI | [click](https://click.palletsprojects.com/) / [typer](https://typer.tiangolo.com/) | CLI applications |
| Validation | [pydantic](https://docs.pydantic.dev/) | Data validation with type hints |

## Minimal example

```python
# hello.py
def greet(name: str = "world") -> str:
    return f"Hello, {name}!"

if __name__ == "__main__":
    print(greet("Python"))
```

```bash
python hello.py
# Output: Hello, Python!
```

## Further reading

- [The Zen of Python](https://peps.python.org/pep-0020/) — guiding principles for Python design
- [PEP 8 Style Guide](https://peps.python.org/pep-0008/) — official style conventions
- [Python Packaging User Guide](https://packaging.python.org/) — packaging and distribution

## Resources

- Python official documentation — https://docs.python.org/3/
- PEP 8 (style guide) — https://peps.python.org/pep-0008/
- Python Packaging Guide — https://packaging.python.org/
- Type hints / typing docs — https://docs.python.org/3/library/typing.html
- Testing guidance (pytest) — https://docs.pytest.org/
- Security guidance (OWASP Python) — https://cheatsheetseries.owasp.org/cheatsheets/Python_Cheat_Sheet.html
- Ruff linter/formatter — https://docs.astral.sh/ruff/
- mypy type checker — https://mypy.readthedocs.io/
