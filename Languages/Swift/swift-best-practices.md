# Swift Best Practices

A concise guide for writing maintainable, performant, and idiomatic Swift code. These practices are based on Apple's official Swift documentation, Swift.org guidelines, and community-established conventions.

## Overview

Swift is Apple's modern programming language designed for safety, performance, and expressiveness. It combines powerful type inference with a clean, readable syntax, making it ideal for developing iOS, macOS, watchOS, and tvOS applications, as well as server-side development.

Swift emphasizes memory safety through features like optionals, automatic reference counting (ARC), and value types. The language promotes clear, self-documenting code through descriptive naming conventions and strong type safety.

## When to use Swift in projects

Swift is ideal for:
- iOS, macOS, watchOS, and tvOS app development
- Server-side applications using frameworks like Vapor or Perfect
- Command-line tools and utilities for Apple platforms
- Performance-critical applications requiring memory safety
- Projects requiring seamless Objective-C interoperability
- Modern applications leveraging SwiftUI and Combine frameworks

## Tooling & ecosystem

### Core Tools
- **Xcode**: Primary IDE with integrated Swift compiler, debugger, and Interface Builder
- **Swift Package Manager (SPM)**: Official dependency management and build system
- **swift-format**: Apple's official code formatter
- **SwiftLint**: Popular linting tool for style and convention enforcement

### Package Managers
- **Swift Package Manager**: Native, integrated with Xcode and command line
- **CocoaPods**: Ruby-based, widely adopted in iOS community
- **Carthage**: Decentralized dependency manager

## Recommended formatting & linters

### SwiftLint Configuration
```yaml
# .swiftlint.yml
line_length: 100
type_body_length: 300
function_body_length: 50
opt_in_rules:
  - empty_count
  - force_unwrapping
  - implicitly_unwrapped_optional
included:
  - Sources
excluded:
  - Carthage
  - Pods
```

### Setup Commands
```bash
# Install SwiftLint via Homebrew
brew install swiftlint

# Install swift-format
brew install swift-format
```

## Testing & CI recommendations

### CI Pipeline Example
```yaml
name: Swift CI
on: [push, pull_request]
jobs:
  test:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v3
    - name: Run tests
      run: swift test
    - name: Run SwiftLint
      run: swiftlint lint --reporter github-actions-logging
```

### Testing Best Practices
- Use XCTest for unit testing
- Follow Arrange-Act-Assert pattern
- Write descriptive test names
- Mock external dependencies using protocols
- Test edge cases and error conditions

## Packaging & release guidance

### Swift Package Manager
```swift
// Package.swift
// swift-tools-version:5.7
import PackageDescription

let package = Package(
    name: "YourPackage",
    platforms: [.iOS(.v13), .macOS(.v10_15)],
    products: [
        .library(name: "YourPackage", targets: ["YourPackage"])
    ],
    targets: [
        .target(name: "YourPackage"),
        .testTarget(name: "YourPackageTests", dependencies: ["YourPackage"])
    ]
)
```

### Release Process
- Follow semantic versioning (SemVer)
- Tag releases with git tags
- Use GitHub Releases for distribution

## Security & secrets best practices

### Secure Coding
```swift
// Use Keychain for sensitive data storage
import Security

func storePassword(_ password: String, for account: String) {
    let data = password.data(using: .utf8)!
    let query: [String: Any] = [
        kSecClass as String: kSecClassGenericPassword,
        kSecAttrAccount as String: account,
        kSecValueData as String: data
    ]
    SecItemAdd(query as CFDictionary, nil)
}

// Avoid hardcoded secrets
let apiKey = ProcessInfo.processInfo.environment["API_KEY"] ?? ""
```

### Security Guidelines
- Never commit secrets to version control
- Use Keychain Services for sensitive data
- Validate all user inputs
- Use App Transport Security (ATS)
- Implement certificate pinning for critical APIs

## Recommended libraries

### Networking
- **Alamofire**: Elegant HTTP networking library
- **URLSession**: Native networking with async/await support

### UI Development
- **SwiftUI**: Declarative UI framework
- **SnapKit**: DSL for Auto Layout constraints

### Testing
- **Quick & Nimble**: BDD-style testing framework

## Minimal example

### Hello World
```swift
// Sources/HelloWorld/main.swift
import Foundation

@main
struct HelloWorld {
    static func main() {
        print("Hello, Swift world!")
        
        let greeting = generateGreeting(for: "Developer")
        print(greeting)
    }
    
    static func generateGreeting(for name: String) -> String {
        guard !name.isEmpty else {
            return "Hello, anonymous!"
        }
        return "Hello, \(name)! Welcome to Swift development."
    }
}
```

### Build and Test Commands
```bash
# Build the project
swift build

# Run tests
swift test

# Run the executable
swift run HelloWorld

# Format code
swift-format format --in-place Sources/

# Lint code
swiftlint lint
```

## Further reading

### Official Documentation
- Swift.org Language Guide — https://docs.swift.org/swift-book/
- Swift API Design Guidelines — https://swift.org/documentation/api-design-guidelines/
- Swift Package Manager Documentation — https://docs.swift.org/package-manager/

### Style Guides
- Ray Wenderlich Swift Style Guide — https://github.com/raywenderlich/swift-style-guide
- Airbnb Swift Style Guide — https://github.com/airbnb/swift

### Community Resources
- Swift Forums — https://forums.swift.org/
- Swift by Sundell — https://www.swiftbysundell.com/
- Hacking with Swift — https://www.hackingwithswift.com/

## Resources

- Swift.org documentation: https://docs.swift.org/swift-book/
- Swift API Design Guidelines: https://swift.org/documentation/api-design-guidelines/
- Swift Package Manager docs: https://docs.swift.org/package-manager/
- Testing guidance (XCTest): https://developer.apple.com/documentation/xctest
- Security guidance (Apple platform security): https://developer.apple.com/security/

---

*This guide synthesizes best practices from Apple's official Swift documentation and community style guides. Always refer to the latest official documentation for current guidance.*