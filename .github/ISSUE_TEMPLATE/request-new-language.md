---
name: Request a new language
about: Request a new programming language for BestPracticesMcp. Provide only a language name and optional authoritative sources; Copilot or maintainers can handle the rest.
title: "[Language request] <INSERT LANGUAGE NAME HERE>"
labels: enhancement, needs-triage
assignees: ''
---

Please fill only the fields below. That's all we need from you.

## Language
- Language name (required): <INSERT LANGUAGE NAME HERE>

## Authoritative documentation sources (optional)
Provide one URL per line (official docs, language website, RFCs, major style guides):
- 
- 
- 

---

## COPILOT INSTRUCTIONS (automation only, submitter please DO NOT EDIT)

```COPILOT
1) Read the "Language name" and any provided authoritative URLs.
   - If any Authoritative Documentation Sources are provided above, include them as references.
   - Even if there are Authoritative Documentation Sources are provided, search the internet for 3-5 (total) sources for reference documentation.
2) Create a best-practices Markdown file at: Languages/<Language>/<language>-best-practices.md
   - Target ~150 lines (100–250 acceptable).
   - Sections to include (in order):
     - Short overview (1–2 paragraphs)
     - When to use this language in projects
     - Tooling & ecosystem (compilers, package managers, linters/formatters)
     - Recommended formatting & linters with config snippets
     - Testing & CI recommendations with example commands
     - Packaging & release guidance
     - Security & secrets best practices
     - Recommended libraries (1–2 per common need)
     - Minimal example (hello world + brief build/test CI snippet)
     - Further reading: include any provided authoritative URLs
   - Attribute sources and include brief license/attribution notes where needed.
3) Add a C# MCP tool at: Languages/<Language>/<Language>.cs
   - Tool must expose the markdown content via the MCP SSE endpoint, matching existing patterns in the repo.
   - Keep the tool minimal (return the file contents, with simple in-memory caching).
4) Update README.md
   - Add the language to the "Included languages" list and include the resource path and MCP tool path.
   - Include a one-line description and link to the markdown file.
5) Validate build & formatting
   - Run: dotnet build BestPracticesMcp.sln
   - Run: dotnet format BestPracticesMcp.sln --verify-no-changes (or fix formatting if needed)
6) Open a PR containing the changes
   - PR title: "Add <Language> best practices (Copilot-generated)" or similar
   - PR body: list authoritative sources used and note any manual checks required.

Notes:
- If a provided source has restrictive licensing, include a short note in the markdown and avoid copying verbatim.
- Keep generated content original; favor summaries and links to authoritative docs.
```

Thank you — provide the language name and any links above and a maintainer or Copilot automation can take it from there.
