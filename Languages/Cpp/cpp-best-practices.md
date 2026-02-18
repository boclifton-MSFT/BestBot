---
language: "C++"
language_version: "C++23"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://isocpp.org/std/the-standard"
---

# C++ Best Practices

A comprehensive guide to writing modern, maintainable, and secure C++ code. These practices are derived from authoritative sources including the C++ Core Guidelines, Google C++ Style Guide, and ISO C++ standards documentation.

## Overview

C++ is a powerful systems programming language that provides low-level control with high-level abstractions. Modern C++ (C++11 and later) offers significant improvements in safety, expressiveness, and performance through features like smart pointers, move semantics, and type deduction.

C++ excels in performance-critical applications where you need direct hardware access, predictable performance, and minimal runtime overhead. The language's zero-cost abstraction principle allows you to write expressive code without sacrificing performance.

## When to use C++ in projects

**Ideal for:**
- System-level programming (operating systems, drivers, embedded systems)
- Performance-critical applications (games, trading systems, scientific computing)
- Real-time systems requiring predictable performance
- Applications requiring fine-grained memory control
- Cross-platform libraries and frameworks
- Legacy system integration where C compatibility is needed

**Consider alternatives when:**
- Rapid prototyping is prioritized over performance
- Development team lacks C++ expertise
- Memory safety is more critical than performance (consider Rust)
- Simple web services or applications where productivity is key

## Tooling & ecosystem

**Compilers:**
- GCC 11+ or Clang 13+ for modern C++ support
- MSVC 2022 for Windows development
- Enable all warnings: `-Wall -Wextra -Wpedantic` (GCC/Clang)

**Build systems:**
- CMake 3.20+ for cross-platform builds
- Bazel for large-scale projects
- Meson for faster builds

**Package managers:**
- vcpkg for cross-platform dependency management
- Conan for C++ package management
- Built-in package manager support in CMake 3.24+

**Static analysis:**
- clang-tidy for comprehensive static analysis
- cppcheck for additional bug detection
- PVS-Studio for enterprise-grade analysis

## Recommended formatting & linters

**Code formatting:**
```cmake
# .clang-format
BasedOnStyle: Google
IndentWidth: 2
ColumnLimit: 80
```

**CMake configuration for clang-tidy:**
```cmake
set(CMAKE_CXX_CLANG_TIDY clang-tidy;
    -checks=*,-fuchsia-*,-google-readability-todo,-modernize-use-trailing-return-type)
```

**Key formatting rules:**
- Use 2-space indentation
- 80-character line limit
- K&R brace style for functions, Allman for classes
- snake_case for variables, PascalCase for types

## Testing & CI recommendations

**Unit testing frameworks:**
```bash
# Google Test setup
find_package(GTest REQUIRED)
target_link_libraries(your_test GTest::gtest_main)

# Run tests
ctest --output-on-failure
```

**CI pipeline example:**
```yaml
# GitHub Actions
- name: Build and Test
  run: |
    cmake -B build -DCMAKE_BUILD_TYPE=Release
    cmake --build build -j$(nproc)
    cd build && ctest --output-on-failure
```

**Testing best practices:**
- Write unit tests for all public interfaces
- Use mock frameworks like GoogleMock for dependencies
- Aim for >90% code coverage
- Include both positive and negative test cases
- Test edge cases and error conditions

## Packaging & release guidance

**CMake packaging:**
```cmake
# CMakeLists.txt
include(GNUInstallDirs)
install(TARGETS your_library
    EXPORT your_library-targets
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR})
```

**Version management:**
- Use semantic versioning (MAJOR.MINOR.PATCH)
- Tag releases in git: `git tag -a v1.2.3 -m "Version 1.2.3"`
- Generate and distribute debug symbols separately
- Provide both static and shared library variants

## Security & secrets best practices

**Memory safety:**
- Use smart pointers (`std::unique_ptr`, `std::shared_ptr`) instead of raw pointers
- Prefer stack allocation over heap when possible
- Use containers (`std::vector`, `std::array`) instead of C-style arrays
- Enable AddressSanitizer in debug builds: `-fsanitize=address`

**Input validation:**
```cpp
// Validate input parameters
if (input.empty() || input.size() > MAX_SIZE) {
    throw std::invalid_argument("Invalid input size");
}
```

**Secure coding:**
- Never use `gets()`, prefer `std::getline()`
- Validate all external input
- Use `std::string` instead of `char*` for text
- Clear sensitive data explicitly with `std::fill()` or `sodium_memzero()`

## Recommended libraries

**Foundation libraries:**
- **Abseil**: Google's collection of C++ library code (strings, containers, algorithms)
- **Boost**: Peer-reviewed portable C++ source libraries

**Networking:**
- **libcurl**: Multi-protocol file transfer library
- **Asio**: Cross-platform C++ library for network programming

**JSON/serialization:**
- **nlohmann/json**: Modern C++ JSON library
- **Protocol Buffers**: Language-neutral data serialization

**GUI/graphics:**
- **Qt**: Cross-platform application framework
- **FLTK**: Lightweight GUI toolkit

## Minimal example

**Hello World with CMake:**

```cpp
// main.cpp
#include <iostream>
#include <string>

int main() {
    const std::string message = "Hello, Modern C++!";
    std::cout << message << std::endl;
    return 0;
}
```

**CMakeLists.txt:**
```cmake
cmake_minimum_required(VERSION 3.20)
project(hello_cpp VERSION 1.0.0)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(hello_cpp main.cpp)

# Enable testing
enable_testing()
add_subdirectory(tests)
```

**Build and test:**
```bash
mkdir build && cd build
cmake ..
make -j$(nproc)
./hello_cpp
ctest
```

## Further reading

**Authoritative sources:**
- C++ Core Guidelines: https://isocpp.github.io/CppCoreGuidelines/
- Google C++ Style Guide: https://google.github.io/styleguide/cppguide.html
- ISO C++ Standards: https://isocpp.org/std/the-standard
- cppreference.com: https://en.cppreference.com/
- Effective Modern C++ by Scott Meyers
- C++ Concurrency in Action by Anthony Williams

**Community resources:**
- CppCon conference talks: https://www.youtube.com/user/CppCon
- C++ Standards Committee papers: https://www.open-std.org/jtc1/sc22/wg21/
- Modern C++ features guide: https://github.com/AnthonyCalandra/modern-cpp-features

## Resources

- C++ Core Guidelines: https://isocpp.github.io/CppCoreGuidelines/
- cppreference: https://en.cppreference.com/
- Google C++ Style Guide: https://google.github.io/styleguide/cppguide.html
- vcpkg: https://github.com/microsoft/vcpkg
- GoogleTest (testing): https://github.com/google/googletest
- CERT C++ Secure Coding Standard: https://wiki.sei.cmu.edu/confluence/display/cplusplus/SEI+CERT+C%2B%2B+Coding+Standard