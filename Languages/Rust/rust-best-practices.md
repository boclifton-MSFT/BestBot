# Rust Best Practices

A concise, opinionated checklist for writing safe, performant, and maintainable Rust code. These practices focus on language features, tooling, and project structure to help you leverage Rust's ownership system and ecosystem effectively.

## Quick checklist

- Embrace ownership and borrowing; design APIs that make invalid states unrepresentable.
- Use `clippy` for linting and let it guide you to idiomatic Rust patterns.
- Prefer `Result<T, E>` for error handling; avoid `panic!` in library code.
- Use `cargo fmt` for consistent formatting and `cargo test` for comprehensive testing.
- Write comprehensive documentation with `///` comments and examples that run as tests.
- Prefer composition over inheritance; use traits for shared behavior.
- Use structured error types with the `thiserror` crate for libraries.
- Keep dependencies minimal; prefer std library when possible.
- Use `#[derive(Debug)]` liberally and implement `Display` for user-facing types.
- Design for zero-cost abstractions; measure performance when optimization matters.

---

## When to use Rust in projects

Rust excels in scenarios requiring both safety and performance: systems programming, network services, CLI tools, WebAssembly applications, blockchain, game engines, and scientific computing. It may not be ideal for rapid prototyping or simple scripts where development speed is prioritized over runtime performance.

## Tooling and ecosystem

**Core tools**: rustc (compiler), cargo (package manager/build tool), rustup (toolchain manager)
**Essential dev tools**: clippy (linting), rustfmt (formatting), rust-analyzer (IDE support), cargo-audit (security scanning)

## Recommended formatting and linters

### rustfmt.toml configuration
```toml
edition = "2021"
max_width = 100
tab_spaces = 4
```

### Clippy configuration in Cargo.toml
```toml
[lints.clippy]
unwrap_used = "deny"
expect_used = "warn"
panic = "deny"
missing_docs_in_private_items = "warn"
```

Run: `cargo clippy -- -D warnings`

## Testing and CI recommendations

### Testing commands
```bash
cargo test              # Run all tests
cargo test --doc        # Run doctests
cargo clippy -- -D warnings
cargo fmt --check
```

### Basic CI example (.github/workflows/ci.yml)
```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: dtolnay/rust-toolchain@stable
      with:
        components: rustfmt, clippy
    - run: cargo fmt --check
    - run: cargo clippy -- -D warnings
    - run: cargo test
```

## Packaging and release guidance

### Essential Cargo.toml
```toml
[package]
name = "your-crate"
version = "0.1.0"
edition = "2021"
license = "MIT OR Apache-2.0"
description = "A brief description"
repository = "https://github.com/user/repo"
```

Release: Update version, tag (`git tag v0.1.0`), publish (`cargo publish`)

## Security and secrets best practices

- Use environment variables for sensitive data; never commit secrets
- Run `cargo audit` regularly to check for vulnerabilities
- Use `zeroize` crate to clear sensitive data from memory
- Validate all external inputs early and convert to strongly typed objects

## Recommended libraries

**Error handling**: thiserror (libraries), anyhow (applications)  
**Serialization**: serde + serde_json  
**Async**: tokio or async-std  
**CLI**: clap + env_logger  
**Web**: axum (server), reqwest (client)  
**Testing**: proptest (property-based), criterion (benchmarking)

## Minimal example

### Hello World with error handling
```rust
// src/main.rs
use std::env;
use std::process;

fn main() {
    let args: Vec<String> = env::args().collect();
    
    if let Err(e) = run(&args) {
        eprintln!("Error: {}", e);
        process::exit(1);
    }
}

fn run(args: &[String]) -> Result<(), Box<dyn std::error::Error>> {
    if args.len() < 2 {
        return Err("Please provide a name".into());
    }
    
    println!("Hello, {}!", args[1]);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_run_with_name() {
        let args = vec!["program".to_string(), "World".to_string()];
        assert!(run(&args).is_ok());
    }

    #[test]
    fn test_run_without_name() {
        let args = vec!["program".to_string()];
        assert!(run(&args).is_err());
    }
}
```

### Cargo.toml
```toml
[package]
name = "hello-rust"
version = "0.1.0"
edition = "2021"

[dependencies]
```

### Build and test commands
```bash
# Build
cargo build

# Run
cargo run -- "World"

# Test
cargo test

# Format and lint
cargo fmt
cargo clippy
```

## Further reading

- [The Rust Programming Language](https://doc.rust-lang.org/book/) - Official book
- [Rust API Guidelines](https://rust-lang.github.io/api-guidelines/) - Best practices for library APIs
- [The Rustonomicon](https://doc.rust-lang.org/nomicon/) - Advanced topics and unsafe code
- [Rust by Example](https://doc.rust-lang.org/rust-by-example/) - Learn by examples
- [Clippy lints](https://rust-lang.github.io/rust-clippy/master/) - Complete list of linting rules

## Resources

- The Rust Book: https://doc.rust-lang.org/book/
- Rust docs and API: https://doc.rust-lang.org/
- Clippy: https://github.com/rust-lang/rust-clippy