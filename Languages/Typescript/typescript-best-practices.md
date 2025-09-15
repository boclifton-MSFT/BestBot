# TypeScript Best Practices

A concise, opinionated checklist for writing maintainable, type-safe, and scalable TypeScript code. These practices focus on language features, tooling, and project configuration and are intended to complement framework-specific guidance.

## Quick checklist

- Enable strict type checking (`"strict": true`) in `tsconfig.json`.
- Prefer specific types over `any`; use `unknown` for untrusted input and narrow types explicitly.
- Use ESLint with `@typescript-eslint` and an automated formatter like Prettier.
- Prefer `readonly` and `const` for immutable data; prefer `as const` for literal narrowing.
- Prefer `interface` for public/extendable APIs and `type` for unions/aliases and complex compositions.
- Use discriminated unions and exhaustive switch checks for robust pattern matching.
- Avoid `namespace` and `module` keywords for new code; use ES module imports/exports.
- Keep functions small and explicitly type public function signatures (parameters and return types).
- Run type checks (`tsc --noEmit`) in CI and before releases.
- Emit declaration files (`declaration: true`) for libraries and keep `declarationMap` when useful.

---

## Project configuration and compiler options

- Turn on `strict` in `tsconfig.json` to enable the recommended set of checks (includes `noImplicitAny`, `strictNullChecks`, `strictFunctionTypes`, etc.).
- Set `target` to a reasonable ECMAScript version (for modern apps, `ES2020` or newer) and set `lib` to match required runtime APIs.
- Use `module` = `ESNext` (or `CommonJS` for Node.js build targets) and prefer native ES modules for new projects.
- Use `moduleResolution: "bundler"` or `"node"` depending on tooling; prefer `bundler` for modern bundlers.
- Enable `esModuleInterop: true` and `skipLibCheck: true` to avoid common interop and third-party typing issues while keeping type safety elsewhere.
- Consider `isolatedModules: true` for tooling that requires single-file transpilation (Babel, SWC); this enforces certain coding patterns (e.g., no type-only const enums).
- For large codebases, use `composite` and `references` to enable project references and faster incremental builds.

## Types and APIs

- Prefer specific types over `any`. If you must accept arbitrary input, use `unknown` and perform runtime checks before narrowing.
- Always type public function/method signatures (including async functions) with parameter and return types. Relying on inference for internal, private helpers is fine but prefer explicitness on boundaries.
- Prefer `readonly` properties and `ReadonlyArray<T>` where possible to document immutability.
- Use `Record<K, V>` for map-like objects when appropriate.
- Prefer `interface` for describing public contracts and `type` aliases for unions, intersections, and mapped types; both are useful—pick consistently according to your project's conventions.
- Use discriminated unions (tagged unions) for sum types and ensure exhaustive checks in `switch` statements (use `never`-based exhaustiveness helpers when needed).
- Use `as const` for tuples and literal objects that should be treated as literal types.
- Avoid excessive use of type assertions (`as`)—they opt-out of the type checker. When assertions are necessary, add a comment explaining why.

## Nullability and optionals

- Prefer `strictNullChecks` and design APIs that model absence explicitly (`T | undefined` or `T | null`) instead of relying on implicit `any` or missing checks.
- Use `?.` (optional chaining) and `??` (nullish coalescing) for concise, intention-revealing handling of optional values.
- Validate external inputs early and convert them to strongly typed domain objects as soon as possible.

## Async patterns

- Always type a function that returns a promise (e.g., `async function foo(): Promise<Result> { ... }`).
- Prefer `try`/`catch` within `async` functions when exceptions are expected and should be handled; propagate errors otherwise.
- Avoid creating long chains of untyped `any`-promises; type intermediate values to preserve correctness.

## Linting and formatting

- Use ESLint with the `@typescript-eslint` plugin and recommended rules (`recommended`, `recommended-requiring-type-checking`) to get both stylistic and type-aware linting.
- Integrate Prettier for formatting and use ESLint-Prettier integration to avoid conflicts (e.g., `eslint-config-prettier`).
- Enforce rules to ban or warn on: `no-explicit-any` (or restrict `any`), inconsistent type imports (`consistent-type-imports`), no-floating-promises, explicit-function-return-type for public APIs, and naming conventions.
- Run linters and formatters locally (pre-commit hooks) and in CI to keep the codebase consistent.

## Code organization and module boundaries

- Keep modules small and focused; export a well-documented public surface per module.
- Avoid deep relative import chains (e.g., `../../../../`); use path aliases (`paths`) or package boundary enforcement to keep module resolution readable.
- Prefer explicit re-exports in index files and be deliberate with “barrel” files—barrels can simplify imports but may increase bundle size or circular dependency risk.
- Co-locate related types, tests, and implementation when it improves discoverability (e.g., `component.ts`, `component.test.ts`, `component.css`).

## Generics and advanced types

- Use generics to write reusable, type-safe abstractions, but keep generic signatures small and well-documented.
- Avoid overly complex mapped types in most application code; prefer clear, easy-to-read type declarations.
- Prefer utility types from `UtilityTypes` (or standard library types) over reimplementing common patterns.

## Testing and CI

- Run a type-check step in CI (`tsc --noEmit`) to catch regressions early.
- Use test runners that work well with TypeScript (Jest + ts-jest, Vitest, or Mocha with ts-node/esm). Configure tests to run in the same module and transpilation mode as production code.
- Mock types carefully; prefer integration tests for complex behaviors that rely on typed data structures.

## Build, packaging, and publishing

- For libraries, emit declaration files (`declaration: true`) and consider `declarationMap: true` to aid debugging for consumers.
- Keep `main`, `module`, and `types` fields in `package.json` accurate and use tools like `tsup`, `esbuild`, or `tsc` + rollup for consistent bundles.
- Avoid shipping `tsconfig` or internal source files in published packages unless necessary; include only what's required for consumers.

## Performance and tooling

- Use `incremental: true` and `tsBuildInfoFile` to speed up subsequent builds.
- Limit `rootDirs` and large `include` globs to reduce watch and project-load times for editors and CI.
- Use `skipLibCheck: true` to avoid long type-checking times for third-party libraries while still maintaining strong checks on your code.

## Common pitfalls and anti-patterns

- Don’t rely on `any` to silence the type system—this erodes the benefits of TypeScript.
- Avoid mixing `require` and `import` in the same module; be consistent with module formats.
- Avoid deeply nested `unknown` casts that bypass compiler guarantees.
- Resist the temptation to annotate everything when inference suffices; prefer explicitness on public boundaries.

## Security and input validation

- Treat all external input as untrusted: validate and sanitize before using, even when types exist (types describe shape, not safety).
- Prefer runtime validation libraries (Zod, io-ts, yup) at public boundaries to convert and validate unknown input into typed domain objects.

---

## Resources

- Official TypeScript docs: https://www.typescriptlang.org/docs/
- TypeScript Handbook: https://www.typescriptlang.org/docs/handbook/intro.html
- ESLint TypeScript plugin: https://github.com/typescript-eslint/typescript-eslint
- Prettier: https://prettier.io/
- Incremental builds & project references: https://www.typescriptlang.org/tsconfig#projectReferences
- Runtime type-validation (examples): Zod (https://github.com/colinhacks/zod), io-ts (https://github.com/gcanti/io-ts)
- Testing guidance (Jest): https://jestjs.io/docs/getting-started
- Security guidance (Node.js/TypeScript): https://cheatsheetseries.owasp.org/cheatsheets/Nodejs_Security_Cheat_Sheet.html

---

This document is intentionally concise—use it as a starting checklist and adapt rules to your project's needs. Keep `tsconfig` and linting rules in source control and run checks in CI to ensure they remain effective.
