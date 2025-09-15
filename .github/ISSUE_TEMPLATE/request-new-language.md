---
name: Request a new language
about: Suggest a new programming language to include in BestPracticesMcp. Provide required resources and info to make adding the language straightforward for maintainers.
title: "[Language request] %s"
labels: enhancement, needs-triage
assignees: ''
---

Thank you for proposing a new language for BestPracticesMcp. To help maintainers evaluate and implement your request quickly, please provide the information below.

### 1) Language
- Language name: <!-- e.g. Rust, Go, Kotlin -->
- Short description / ecosystem notes: <!-- Why this language should be included? -->

### 2) Proposed resource files (required)
- Best-practices markdown (required): Languages/<Language>/<language>-best-practices.md
  - Please attach a draft of the Markdown file or a link to a Gist/branch that contains it.
  - Target length: ~100–250 lines. Focus on high-value guidance (idioms, tooling, testing, formatting, packaging, security, best libraries).
- MCP tool file (required): Languages/<Language>/<Language>.cs (or Functions/<Language>Tools.cs)
  - Provide a short skeleton or note if you want maintainers to create it. The MCP tool must return the markdown content to the MCP SSE endpoint.

> NOTE: When adding a language, the repository's Copilot automation requires the PR to also update the README to list the new language and the resource/tool paths. Please include a suggested README snippet if possible.

### 3) Licensing & attribution
- Is the content original, or does it include material from external sources? If external, include license and attribution details.

### 4) Acceptance criteria / checklist
- [ ] Draft best-practices markdown attached or linked
- [ ] MCP tool skeleton or implementation attached or linked
- [ ] Confirm license/attribution is compatible with this repository
- [ ] Provide a suggested README.md entry with the resource path and tool path
- [ ] Optional: links to example projects, CI, or tests demonstrating recommendations

### 5) Validation steps for maintainers (suggested)
Maintainers will typically do the following in the same PR that adds the language:
- Add the Markdown file to Languages/<Language>/<language>-best-practices.md
- Add the MCP tool to Languages/<Language>/<Language>.cs (or Functions/<Language>Tools.cs) following existing patterns
- Update README.md "Included languages" section with the language and resource/tool paths
- Run `dotnet build BestPracticesMcp.sln` and `dotnet format --verify-no-changes` to ensure the solution builds and is formatted
- Verify the new resource is accessible via the MCP endpoint (local functions host or CI run)

If you would like maintainers to implement the language for you, indicate that below and attach the content/resources. Maintainers will triage based on bandwidth and applicability.

### 6) Additional notes
- If the language has multiple variants (e.g. Node/TypeScript), indicate which variant you are requesting.
- If this is a translation of an existing entry, include original source and translator info.

---

Thank you — a maintainer will triage this request once submitted.
