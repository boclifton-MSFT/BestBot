---
name: add-language
description: Add a new programming language or framework to the BestBot MCP server. Use when a user asks to add, create, or scaffold a new language/framework in BestBot, or when processing a "Request a new language" issue. Triggers: "add language", "new language", "add framework", "add best practices", "request new language", "scaffold language", "add <language> to BestBot".
---

# Add Language to BestBot

Automated workflow for adding a new programming language or framework to the BestBot MCP server. This skill handles research, content creation, code scaffolding, README updates, and build validation.

> **Input required:** The language or framework name. Optionally, the user may provide authoritative documentation URLs.

---

## Workflow Overview

Adding a language requires exactly four artifacts and one validation step:

| Step | Artifact | Path |
|------|----------|------|
| 1 | Research best practices | (web search + synthesis) |
| 2 | Best-practices markdown | `Languages/<Language>/<language>-best-practices.md` |
| 3 | C# MCP tool class | `Languages/<Language>/<Language>.cs` |
| 4 | README update | `README.md` — "Included languages" section |
| 5 | Build & format validation | `dotnet build` + `dotnet format --verify-no-changes` |

---

## Step 1: Research Best Practices

**This step is critical.** Before writing any content, gather authoritative sources.

### 1a. Collect user-provided sources

If the user or issue provides authoritative documentation URLs, note them for use as primary references.

### 1b. Search for authoritative documentation

Use `fetch_webpage` to retrieve content from the language's official documentation sites. Search for **3–5 total authoritative sources** covering:

- **Official language website / docs** (e.g., `https://go.dev/doc/`, `https://docs.python.org/`)
- **Official style guide or design guidelines** (e.g., `https://peps.python.org/pep-0008/`)
- **Recommended linters/formatters** and their documentation
- **Testing frameworks and CI guidance**
- **Security best practices** for the language ecosystem

**Search strategy for finding sources:**

1. Start with the language's official website: `https://<language>.org` or equivalent
2. Check for official style guides, coding conventions, or design documents
3. Look for the ecosystem's primary package manager documentation
4. Find the recommended linter/formatter tools and their config references
5. Identify the standard testing framework and CI patterns

**Example source discovery for a language like Scala:**
```
fetch_webpage urls:
  - https://docs.scala-lang.org/style/
  - https://docs.scala-lang.org/tutorials/
  - https://www.scala-lang.org/documentation/
  - https://docs.scala-lang.org/overviews/core/
```

### 1c. Synthesize research

After gathering sources, extract and organize key information into these categories:
- Overview and philosophy
- Use cases and strengths
- Tooling (compiler/interpreter, package manager, formatter, linter)
- Testing ecosystem
- Security considerations
- Popular/recommended libraries

---

## Step 2: Create Best-Practices Markdown

**Location:** `Languages/<Language>/<language>-best-practices.md`

**Naming conventions:**
- Folder name: PascalCase (e.g., `Go`, `Vue3`, `NetAspire`, `Cpp`)
- File name: lowercase-kebab-case with `-best-practices.md` suffix
- Match existing patterns in the repository

**Target length:** ~150 lines (100–250 acceptable)

**Required sections (in order):**

```markdown
# <Language> Best Practices

<1–2 paragraph overview: what the language is, philosophy, key strengths>

## When to use <Language> in projects

<Bullet list of ideal use cases>

## Tooling & ecosystem

### Core tools
<Compiler/interpreter, package manager, formatter, linter>

### Package management
<Commands for init, install, update>

## Recommended formatting & linters

<Tool names, install commands, minimal config snippets>

## Testing & CI recommendations

<Standard test framework, example commands, CI snippet>

## Packaging & release guidance

<How to build/package for distribution>

## Security & secrets best practices

<Language-specific security patterns>

## Recommended libraries

<1–2 libraries per common need, brief descriptions>

## Minimal example

<Hello world + brief build/test snippet>

## Further reading

<Links to key resources>

## Resources

<REQUIRED: Bullet list of all canonical URLs used as references>
```

**Resources section rules:**
- Each resource: `- Title — https://full-url-here`
- URLs must be directly accessible HTTPS links (no placeholders)
- Minimum entries: official docs, style guide, linter/formatter page, testing guidance, security guidance
- Keep entries concise — one line per resource

**Content guidelines:**
- Write original content; summarize and link rather than copying verbatim
- Include brief license/attribution notes if referencing restrictively-licensed content
- Use concrete code examples with language-idiomatic patterns
- Include config file snippets where relevant (e.g., `.eslintrc`, `pyproject.toml`)

---

## Step 3: Create C# MCP Tool Class

**Location:** `Languages/<Language>/<Language>.cs`

Use this exact template, replacing all placeholders:

```csharp
using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the <DISPLAY_NAME> programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class <PASCAL_NAME>Tools(ILogger<<PASCAL_NAME>Tools> logger)
{
    [Function(nameof(Get<PASCAL_NAME>BestPractices))]
    public async Task<string> Get<PASCAL_NAME>BestPractices(
        [McpToolTrigger("get_<SNAKE_NAME>_best_practices", "Retrieves best practices for the <DISPLAY_NAME> programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<<PASCAL_NAME>Tools>.Serving(logger, "get_<SNAKE_NAME>_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "<PASCAL_NAME>", "<KEBAB_NAME>-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<<PASCAL_NAME>Tools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<<PASCAL_NAME>Tools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<<PASCAL_NAME>Tools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<<PASCAL_NAME>Tools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<<PASCAL_NAME>Tools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<<PASCAL_NAME>Tools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# <DISPLAY_NAME> Best Practices",
            "<FALLBACK_LINE_1>",
            "<FALLBACK_LINE_2>",
            "<FALLBACK_LINE_3>",
            "<FALLBACK_LINE_4>",
            "<FALLBACK_LINE_5>"
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
```

**Placeholder reference:**

| Placeholder | Description | Example (for Go) |
|-------------|-------------|-------------------|
| `<PASCAL_NAME>` | PascalCase class/folder name | `Go` |
| `<SNAKE_NAME>` | snake_case for MCP tool name | `go` |
| `<KEBAB_NAME>` | kebab-case for markdown filename | `go` |
| `<DISPLAY_NAME>` | Human-readable name | `Go` |
| `<FALLBACK_LINE_N>` | 5 brief fallback best-practice tips | `"- Use gofmt for formatting."` |

**Special naming cases (follow existing patterns):**

| Language | PASCAL_NAME | SNAKE_NAME | KEBAB_NAME | Folder |
|----------|-------------|------------|------------|--------|
| C++ | Cpp | cpp | cpp | Cpp |
| C# | Csharp | csharp | csharp | Csharp |
| Vue 3 | Vue3 | vue3 | vue3 | Vue3 |
| .NET Aspire | NetAspire | netaspire | netaspire | NetAspire |
| React | React | react | react | React |

---

## Step 4: Update README.md

Add the new language to the **"Included languages"** section in `README.md`, maintaining alphabetical order among the existing entries.

**Format (match existing entries exactly):**

```markdown
- <Display Name>
  - Resource: `Languages/<PascalName>/<kebab-name>-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/<PascalName>.cs`)
```

**Placement:** Insert alphabetically within the existing list. For example, "Haskell" goes between "Go" and "Java".

---

## Step 5: Build & Format Validation

**CRITICAL: Both commands must pass before the work is complete.**

### 5a. Build

```bash
dotnet build BestPracticesMcp.sln
```
- Timeout: 60+ seconds. NEVER CANCEL.
- Must exit with code 0 (no errors).
- Warnings are acceptable but errors are not.

### 5b. Verify formatting

```bash
dotnet format BestPracticesMcp.sln --verify-no-changes
```
- Timeout: 60+ seconds. NEVER CANCEL.
- If this fails (exit code 2), run `dotnet format BestPracticesMcp.sln` to fix, then re-verify.

### 5c. If build fails

Common issues and fixes:
- **Namespace conflict**: Ensure the class is in `BestPracticesMcp.Functions` namespace
- **Missing using**: Add `using BestPracticesMcp.Utilities;`
- **File not found at runtime**: Verify the `.md` file path matches the `Path.Combine(...)` in the C# tool
- **Naming mismatch**: Ensure `nameof(Get<X>BestPractices)` matches the method name exactly

---

## Checklist

Before considering the task complete, verify all of the following:

- [ ] Researched 3–5 authoritative sources for the language
- [ ] Created `Languages/<Language>/<language>-best-practices.md` (100–250 lines)
- [ ] Markdown includes all required sections (overview, tooling, testing, security, resources)
- [ ] Resources section has minimum 5 canonical HTTPS URLs
- [ ] Created `Languages/<Language>/<Language>.cs` matching the template exactly
- [ ] MCP tool name follows pattern: `get_<snake_name>_best_practices`
- [ ] Updated README.md "Included languages" section (alphabetical order)
- [ ] `dotnet build BestPracticesMcp.sln` passes (exit code 0)
- [ ] `dotnet format BestPracticesMcp.sln --verify-no-changes` passes (exit code 0)
