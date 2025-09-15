# C# Best Practices

A concise, opinionated checklist for writing maintainable, reliable, and secure C# code. These are general, language-focused practices meant to complement framework- or platform-specific guidance.

## Quick checklist

- Prefer clarity over cleverness; name things for intent.
- Keep methods small; a function should do one thing well.
- Embrace async/await end-to-end; pass CancellationToken.
- Validate inputs and fail fast with helpful messages.
- Catch only what you can handle; preserve stack traces.
- Use nullable reference types and guard against nulls.
- Favor immutability (records/readonly) for data models.
- Dispose resources deterministically; prefer `await using`.
- Measure before optimizing; avoid premature micro-opts.
- Write tests for behavior and edge cases; keep them stable.

---

## Naming and style

- Use PascalCase for types, methods, and public members; camelCase for locals and parameters.
- Choose descriptive, intent-revealing names; avoid abbreviations and magic numbers.
- Prefer expression-bodied members for short, obvious implementations; otherwise keep method bodies simple.
- Keep files focused: generally one public type per file.

... (document continues with the same content as the original `Resources/csharp-best-practices.md`)
