---
language_version: "25"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://www.oracle.com/java/technologies/downloads/"
---

# Java Best Practices

A comprehensive guide to writing maintainable, performant, and idiomatic Java based on Oracle documentation, the OpenJDK Developers' Guide, and the Google Java Style Guide.

## Overview

Java is a statically typed, object-oriented, platform-independent programming language that runs on the Java Virtual Machine (JVM). With its strong type system, garbage collection, mature ecosystem, and "write once, run anywhere" philosophy, Java powers enterprise backends, Android applications, distributed systems, and large-scale data processing. Modern Java (17+) includes records, sealed classes, pattern matching, and virtual threads, making the language increasingly expressive while retaining backward compatibility.

## When to use Java in projects

- **Enterprise backends and microservices**: Spring Boot, Quarkus, Jakarta EE
- **Android development**: Native Android apps (though Kotlin is now preferred)
- **Large-scale distributed systems**: Kafka, Hadoop, Spark, Cassandra
- **Financial and banking systems**: Mission-critical transactional processing
- **Cloud-native services**: Containerized services on Kubernetes
- **Data-intensive applications**: Batch processing, ETL pipelines
- **API services**: RESTful and gRPC services with strong typing

## Tooling & ecosystem

### Core tools
- **JDK**: OpenJDK (Adoptium/Temurin), Oracle JDK, Amazon Corretto
- **Build tools**: [Maven](https://maven.apache.org/) (convention-based), [Gradle](https://gradle.org/) (flexible, Kotlin DSL)
- **IDE**: IntelliJ IDEA, VS Code with Extension Pack for Java, Eclipse
- **Formatter**: [google-java-format](https://github.com/google/google-java-format), [Spotless](https://github.com/diffplug/spotless)
- **Static analysis**: [SpotBugs](https://spotbugs.github.io/), [Error Prone](https://errorprone.info/), [SonarQube](https://www.sonarsource.com/products/sonarqube/)

### Project setup (Maven)

```bash
mvn archetype:generate \
  -DgroupId=com.example \
  -DartifactId=my-app \
  -DarchetypeArtifactId=maven-archetype-quickstart \
  -DinteractiveMode=false

cd my-app && mvn compile
```

## Recommended formatting & linters

### google-java-format (recommended)

```bash
# Format files
java -jar google-java-format.jar --replace src/**/*.java

# Enforce in CI with Spotless (Maven)
mvn spotless:check
```

Example Spotless Maven plugin config:

```xml
<plugin>
  <groupId>com.diffplug.spotless</groupId>
  <artifactId>spotless-maven-plugin</artifactId>
  <version>2.43.0</version>
  <configuration>
    <java>
      <googleJavaFormat/>
    </java>
  </configuration>
</plugin>
```

### Code style essentials

- Use `UpperCamelCase` for classes, `lowerCamelCase` for methods/variables, `UPPER_SNAKE_CASE` for constants
- One top-level class per file; keep imports explicit (no wildcards)
- Prefer a 100-character column limit and wrap at high syntactic boundaries
- Use canonical Maven/Gradle directory layout (`src/main/java`, `src/test/java`)
- Use records for value objects, sealed classes for restricted hierarchies

```java
public record Point(double x, double y) {
    public double distanceTo(Point other) {
        double dx = this.x - other.x;
        double dy = this.y - other.y;
        return Math.sqrt(dx * dx + dy * dy);
    }
}
```

## Testing & CI recommendations

### JUnit 5 + Mockito

```xml
<dependency>
  <groupId>org.junit.jupiter</groupId>
  <artifactId>junit-jupiter</artifactId>
  <version>5.11.0</version>
  <scope>test</scope>
</dependency>
```

Example test:

```java
import static org.junit.jupiter.api.Assertions.*;
import org.junit.jupiter.api.Test;

class PointTest {
    @Test
    void distanceToSamePointIsZero() {
        var p = new Point(3.0, 4.0);
        assertEquals(0.0, p.distanceTo(p), 1e-9);
    }

    @Test
    void distanceBetweenKnownPoints() {
        var a = new Point(0, 0);
        var b = new Point(3, 4);
        assertEquals(5.0, a.distanceTo(b), 1e-9);
    }
}
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        java-version: [17, 21]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-java@v4
        with:
          java-version: ${{ matrix.java-version }}
          distribution: temurin
      - run: mvn verify -B
      - run: mvn spotless:check
```

## Packaging & release guidance

- Use Maven Central or GitHub Packages for publishing artifacts
- Build reproducible JARs with `maven-jar-plugin`; create fat JARs with `maven-shade-plugin` or Spring Boot plugin
- Ship container images with multi-stage Docker builds using `eclipse-temurin` base images
- Use semantic versioning and maintain a CHANGELOG
- Enable `javac -Werror` for strict compilation in CI

## Security & secrets best practices

- Never embed secrets in source; use environment variables or a secrets manager (Azure Key Vault, HashiCorp Vault)
- Use parameterized queries (PreparedStatement) to prevent SQL injection
- Validate and sanitize all external input; use bean validation (`jakarta.validation`)
- Use `try-with-resources` for `AutoCloseable` resources to prevent leaks
- Keep dependencies updated; scan with OWASP Dependency-Check or Snyk
- Avoid `ObjectInputStream.readObject()` with untrusted data (deserialization attacks)

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Web framework | [Spring Boot](https://spring.io/projects/spring-boot) | De facto standard for microservices |
| HTTP client | [java.net.http.HttpClient](https://docs.oracle.com/en/java/javase/21/docs/api/java.net.http/java/net/http/HttpClient.html) | Built-in since Java 11 |
| JSON | [Jackson](https://github.com/FasterXML/jackson) / [Gson](https://github.com/google/gson) | Serialization / deserialization |
| Testing | [JUnit 5](https://junit.org/junit5/) + [Mockito](https://site.mockito.org/) | Unit and integration testing |
| Logging | [SLF4J](https://www.slf4j.org/) + [Logback](https://logback.qos.ch/) | Facade + implementation |
| Dependency injection | [Spring](https://spring.io/) / [Guice](https://github.com/google/guice) | IoC containers |

## Minimal example

```java
// Hello.java
public class Hello {
    public static void main(String[] args) {
        System.out.println("Hello, Java!");
    }
}
```

```bash
javac Hello.java && java Hello
# Output: Hello, Java!
```

## Further reading

- [Effective Java by Joshua Bloch](https://www.oreilly.com/library/view/effective-java/9780134686097/) — essential item-based guidance for writing better Java
- [Google Java Style Guide](https://google.github.io/styleguide/javaguide.html) — detailed formatting and naming conventions
- [Modern Java in Action](https://www.manning.com/books/modern-java-in-action) — streams, lambdas, and functional programming

## Resources

- Oracle Java documentation — https://docs.oracle.com/en/java/javase/21/
- OpenJDK Developers' Guide — https://openjdk.org/guide/
- Google Java Style Guide — https://google.github.io/styleguide/javaguide.html
- JUnit 5 User Guide — https://junit.org/junit5/docs/current/user-guide/
- Spring Boot reference — https://docs.spring.io/spring-boot/reference/
- Maven documentation — https://maven.apache.org/guides/
- OWASP Java Security Cheat Sheet — https://cheatsheetseries.owasp.org/cheatsheets/Java_Security_Cheat_Sheet.html
- Error Prone static analysis — https://errorprone.info/
