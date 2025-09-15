# Java Best Practices

This document condenses authoritative guidance from Oracle Java documentation, the OpenJDK Developers’ Guide, and the Google Java Style Guide into concise, actionable best-practices for writing maintainable, idiomatic Java.

Sources
- Oracle Java Code Conventions — https://www.oracle.com/java/technologies/javase/codeconvtoc-136057.html
- OpenJDK Developers’ Guide — https://openjdk.org/guide/
- Google Java Style Guide — https://google.github.io/styleguide/javaguide.html

Key Practices

1. Project & Source Layout
- Use a canonical, Maven/Gradle-compatible directory layout (src/main/java, src/test/java).
- Keep package names all-lowercase and reverse-domain prefixed (e.g., com.example.project).

2. Formatting & Style
- Follow a consistent formatter (google-java-format or equivalent) and enforce it in CI.
- Use 2-space or 4-space indentation consistently; prefer 2 spaces only when project-wide convention demands it — default to 4 spaces.
- Respect a column limit (100 chars per Google guide) and wrap long expressions at high syntactic boundaries.
- Avoid wildcard imports; keep imports explicit and sorted.

3. Naming & Structure
- Class names: UpperCamelCase; method/field/variable names: lowerCamelCase; constants: UPPER_SNAKE_CASE.
- One top-level class per file; order members logically and keep related overloads grouped.

4. Immutability & API Design
- Prefer immutable types for value objects (use final fields, private constructors, builders or records when appropriate).
- Keep public APIs stable and document expectations via Javadoc.

5. Exception Handling
- Catch only exceptions you can handle; avoid swallowing exceptions silently.
- Use descriptive exception messages and preserve the original exception as a cause when rethrowing.

6. Concurrency
- Prefer standard concurrency utilities from java.util.concurrent (Executors, CompletableFuture, ConcurrentHashMap) over low-level threads.
- Document thread-safety expectations for public classes.

7. Performance & Resource Management
- Use try-with-resources for AutoCloseable resources to ensure deterministic cleanup.
- Measure before optimizing; prefer clarity over micro-optimizations.

8. Testing
- Write fast, deterministic unit tests; use integration tests only where necessary.
- Favor behavior-focused tests and isolate external dependencies via test doubles or test containers.

9. Tooling
- Use static analysis and linters (SpotBugs, Error Prone, SonarCloud) to catch common defects.
- Enforce formatting and compile checks in CI (google-java-format, javac with -Werror for strictness when desired).

10. Documentation
- Document public APIs using Javadoc; keep summary fragments brief and meaningful.
- Use TODO comments with a bug reference or tracking link when leaving temporary work.

Minimal Fallback (if file unavailable)
- Prefer small, focused classes
- Use clear names and document behavior
- Test behavior and edge cases

## Resources

- Oracle Java documentation: https://docs.oracle.com/javase/
- OpenJDK Guide: https://openjdk.org/guide/
- Google Java Style Guide: https://google.github.io/styleguide/javaguide.html
- Effective Java (Joshua Bloch): https://www.pearson.com/
- Testing guidance (JUnit): https://junit.org/junit5/docs/current/user-guide/
- Security guidance for Java: https://cheatsheetseries.owasp.org/cheatsheets/Java_Security_Cheat_Sheet.html

