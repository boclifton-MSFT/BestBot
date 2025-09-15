# Go Best Practices

A comprehensive guide to writing idiomatic, maintainable, and efficient Go code based on authoritative Go documentation, community standards, and proven patterns from the Go ecosystem.

## Overview

Go is a statically typed, compiled programming language designed for simplicity, performance, and productivity. It excels at building scalable web services, CLI tools, and distributed systems. Go's philosophy emphasizes readability, simplicity, and explicit error handling.

## When to use Go in projects

- **Web services and APIs**: Excellent HTTP support, built-in concurrency, fast startup times
- **CLI tools and utilities**: Single binary deployment, cross-platform compilation
- **Microservices**: Lightweight, fast, good for containerized deployments
- **System programming**: Memory-safe alternative to C/C++ for many use cases
- **Network programming**: Strong standard library for networking protocols
- **DevOps tooling**: Infrastructure tools, build systems, automation scripts

## Tooling & ecosystem

### Core tools
- **Compiler**: `go build`, `go run` for development and building
- **Package manager**: `go mod` for dependency management (built-in since Go 1.11)
- **Formatter**: `gofmt` (built-in, non-negotiable formatting)
- **Linter**: `go vet` (built-in), `golangci-lint` (comprehensive)

### Package management
```bash
go mod init module-name
go mod tidy
go mod download
```

## Recommended formatting & linters

### Formatting (mandatory)
```bash
# Format all Go files
gofmt -w .
# Or using go fmt
go fmt ./...
```

### Linting configuration
```yaml
# .golangci.yml
linters:
  enable:
    - gofmt
    - goimports
    - govet
    - errcheck
    - staticcheck
    - unused
    - gosimple
    - ineffassign
```

### Recommended tools
- **goimports**: Auto-manage imports
- **golangci-lint**: Meta-linter with many checks
- **staticcheck**: Advanced static analysis

## Testing & CI recommendations

### Testing commands
```bash
# Run tests
go test ./...
# Run tests with coverage
go test -cover ./...
# Generate coverage report
go test -coverprofile=coverage.out ./...
go tool cover -html=coverage.out
```

### CI pipeline example
```yaml
# GitHub Actions
- name: Setup Go
  uses: actions/setup-go@v4
  with:
    go-version: '1.21'
- name: Build
  run: go build -v ./...
- name: Test
  run: go test -v ./...
- name: Lint
  run: |
    go install github.com/golangci/golangci-lint/cmd/golangci-lint@latest
    golangci-lint run
```

## Packaging & release guidance

### Building and distribution
```bash
# Build for current platform
go build -o myapp ./cmd/myapp

# Cross-compilation
GOOS=linux GOARCH=amd64 go build -o myapp-linux-amd64 ./cmd/myapp
GOOS=windows GOARCH=amd64 go build -o myapp-windows-amd64.exe ./cmd/myapp
```

### Release best practices
- Use semantic versioning with git tags
- Build statically-linked binaries when possible
- Use `go mod vendor` for reproducible builds
- Consider using GoReleaser for automated releases

## Security & secrets best practices

### Environment variables and secrets
```go
// Use environment variables for configuration
import "os"

dbURL := os.Getenv("DATABASE_URL")
if dbURL == "" {
    log.Fatal("DATABASE_URL environment variable required")
}
```

### Input validation
```go
// Validate and sanitize inputs
func validateEmail(email string) error {
    if len(email) > 254 {
        return errors.New("email too long")
    }
    // Use regex or proper email validation library
    return nil
}
```

### Crypto best practices
- Use `crypto/rand` for random number generation
- Use standard library crypto packages when possible
- Never implement custom crypto algorithms

## Recommended libraries

### Web frameworks
- **net/http**: Standard library (recommended for most use cases)
- **gin-gonic/gin**: Popular HTTP framework for APIs
- **gorilla/mux**: HTTP router and dispatcher

### Database access
- **database/sql**: Standard library with drivers
- **GORM**: Feature-rich ORM
- **sqlx**: Extensions to database/sql

### Utilities
- **logrus** or **zap**: Structured logging
- **cobra**: CLI applications
- **viper**: Configuration management

## Code style and idioms

### Error handling
```go
// Always check errors explicitly
result, err := someFunction()
if err != nil {
    return fmt.Errorf("failed to call someFunction: %w", err)
}
```

### Concurrency patterns
```go
// Use channels for communication
ch := make(chan string, 10)
go func() {
    ch <- "hello"
    close(ch)
}()

// Use context for cancellation
ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
defer cancel()
```

### Naming conventions
- Use **camelCase** for unexported names
- Use **PascalCase** for exported names
- Use **short, descriptive names** for variables
- **Package names** should be lowercase, single words

## Minimal example

### Hello World application
```go
// main.go
package main

import (
    "fmt"
    "log"
    "net/http"
)

func main() {
    http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
        fmt.Fprintf(w, "Hello, World!")
    })
    
    log.Println("Server starting on :8080")
    if err := http.ListenAndServe(":8080", nil); err != nil {
        log.Fatal(err)
    }
}
```

### go.mod file
```go
module hello-world

go 1.21

// No dependencies for this simple example
```

### Build and test commands
```bash
# Initialize module
go mod init hello-world

# Run locally
go run main.go

# Build binary
go build -o hello-world

# Test (if you have tests)
go test ./...
```

### Basic CI snippet
```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-go@v4
      with:
        go-version: '1.21'
    - run: go build -v ./...
    - run: go test -v ./...
```

## Further reading

- **Effective Go**: https://go.dev/doc/effective_go
- **Go Code Review Comments**: https://go.dev/wiki/CodeReviewComments
- **Go Blog**: https://go.dev/blog/
- **Go by Example**: https://gobyexample.com/
- **The Go Programming Language Specification**: https://go.dev/ref/spec

## Resources

- Effective Go: https://go.dev/doc/effective_go
- Go docs and spec: https://go.dev/doc/
- Go by Example: https://gobyexample.com/

## Attribution

This guide synthesizes best practices from the official Go documentation, community standards, and established patterns in the Go ecosystem. Content is original and includes references to authoritative sources.