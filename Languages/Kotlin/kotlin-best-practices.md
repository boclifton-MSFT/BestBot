# Kotlin Best Practices

A concise guide to writing clean, efficient Kotlin code across JVM, Android, Native, and Multiplatform projects.

## Overview

Kotlin is a modern, statically-typed language by JetBrains that's 100% interoperable with Java while offering concise syntax, null safety, and functional programming features. It's Google's preferred language for Android and increasingly popular for server-side and multiplatform development, emphasizing pragmatism, safety, and reduced boilerplate.

## When to use Kotlin

**Ideal for:** Android development, server-side (Spring/Ktor), multiplatform projects, modernizing Java codebases, projects needing null safety, async programming with coroutines

**Consider alternatives for:** Performance-critical systems (C/C++/Rust), pure web frontend, strict memory constraints, teams with no JVM experience

## Tooling & ecosystem

**Targets:** Kotlin/JVM, Kotlin/Android, Kotlin/JS, Kotlin/Native, Kotlin Multiplatform
**Build:** Gradle (preferred, Kotlin DSL), Maven
**IDEs:** IntelliJ IDEA, Android Studio
**Packages:** Maven Central, kotlinx.* official libraries

## Recommended formatting & linters

**ktlint Setup:**
```kotlin
// build.gradle.kts
plugins {
    id("org.jlleitschuh.gradle.ktlint") version "11.0.0"
    id("io.gitlab.arturbosch.detekt") version "1.23.0"
}
```

**Configuration:**
```kotlin
// .editorconfig
[*.{kt,kts}]
indent_style = space
indent_size = 4
max_line_length = 120
```

## Testing & CI recommendations

**Dependencies:**
```kotlin
testImplementation("org.junit.jupiter:junit-jupiter:5.9.2")
testImplementation("io.mockk:mockk:1.13.4")
```

**Example Test:**
```kotlin
class CalculatorTest {
    @Test
    fun `should add two numbers correctly`() {
        assertEquals(5, Calculator().add(2, 3))
    }
}
```

**CI Commands:**
```bash
./gradlew ktlintCheck detekt test
```

## Packaging & release guidance

**Publishing:**
```kotlin
publishing {
    publications {
        create<MavenPublication>("maven") {
            from(components["java"])
        }
    }
}
```

**Best Practices:** Use semantic versioning, tag releases, automate with GitHub Actions

## Security & secrets best practices

**Null Safety:**
```kotlin
fun processUser(user: User?): String = user?.name ?: "Unknown User"
val length = user?.name?.length ?: 0
```

**Secrets:**
```kotlin
// ✅ Use environment variables
val apiKey = System.getenv("API_KEY") 
    ?: throw IllegalStateException("API_KEY not set")
```

**Validation:**
```kotlin
data class User(val email: String) {
    init {
        require(email.isNotBlank() && email.contains("@"))
    }
}
```

## Recommended libraries

**HTTP:** `ktor-client` (multiplatform), `okhttp3` (JVM/Android)
**JSON:** `kotlinx.serialization`, `moshi`
**Async:** `kotlinx.coroutines`
**DI:** `koin`, `dagger-hilt` (Android)
**Database:** `exposed`, `sqldelight`

## Minimal example

**Hello World with build setup:**

```kotlin
// src/main/kotlin/Main.kt
import kotlinx.coroutines.*

data class User(val name: String, val email: String?)

suspend fun fetchUser(id: Int): User {
    delay(100) // Simulate network call
    return User("John Doe", "john@example.com")
}

fun main() = runBlocking {
    println("Hello, Kotlin!")
    
    val user = fetchUser(1)
    println("User: ${user.name}, Email: ${user.email ?: "N/A"}")
    
    // Null safety demo
    val emailLength = user.email?.length ?: 0
    println("Email length: $emailLength")
}
```

**Build file (build.gradle.kts):**
```kotlin
plugins {
    kotlin("jvm") version "1.9.10"
    application
}

repositories {
    mavenCentral()
}

dependencies {
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.3")
    testImplementation("org.junit.jupiter:junit-jupiter:5.9.2")
}

application {
    mainClass.set("MainKt")
}

tasks.test {
    useJUnitPlatform()
}
```

**CI Test Command:**
```bash
./gradlew clean build test
```

## Further reading

- [Kotlin Documentation](https://kotlinlang.org/docs/)
- [Kotlin Coding Conventions](https://kotlinlang.org/docs/coding-conventions.html)
- [Android Kotlin Style Guide](https://developer.android.com/kotlin/style-guide)
- [Coroutines Guide](https://kotlinlang.org/docs/coroutines-guide.html)
- [Awesome Kotlin](https://kotlin.link/)

---

*This guide covers general Kotlin best practices. For platform-specific guidance, consult Android, Spring, or other framework documentation.*