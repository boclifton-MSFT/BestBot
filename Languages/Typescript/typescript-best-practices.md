---
language: "TypeScript"
language_version: "5.7"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://www.typescriptlang.org/download"
---

# TypeScript Best Practices

A comprehensive guide to writing maintainable, type-safe, and scalable TypeScript based on the official TypeScript documentation, community conventions, and widely adopted tooling standards.

## Overview

TypeScript is a statically typed superset of JavaScript developed by Microsoft. It adds optional type annotations, interfaces, generics, and advanced type-system features that catch errors at compile time while emitting standard JavaScript. TypeScript integrates deeply with modern editors to provide IntelliSense, refactoring, and navigation tooling. With first-class support in frameworks like Angular, Next.js, and Deno, TypeScript has become the default choice for large-scale JavaScript projects that benefit from strong typing and maintainability.

## When to use TypeScript in projects

- **Large-scale web applications**: Angular, Next.js, Remix, SvelteKit
- **Node.js services and APIs**: Express, Fastify, NestJS, tRPC
- **Shared libraries and SDKs**: NPM packages with built-in type definitions
- **Monorepos**: Multi-package projects benefiting from project references
- **Full-stack applications**: Shared types between frontend and backend
- **CLI tools**: Type-safe argument parsing and configuration
- **Migration from JavaScript**: Gradual adoption with `allowJs` and `checkJs`

## Tooling & ecosystem

### Core tools
- **Compiler**: `tsc` (built-in), [SWC](https://swc.rs/) (fast transpiler), [esbuild](https://esbuild.github.io/)
- **Package manager**: npm, pnpm, yarn
- **Bundler**: [Vite](https://vitejs.dev/), [tsup](https://tsup.egoist.dev/), Webpack
- **Linter**: [ESLint](https://eslint.org/) with [@typescript-eslint](https://typescript-eslint.io/)
- **Formatter**: [Prettier](https://prettier.io/)

### Project setup

```bash
npm init -y
npm install -D typescript @types/node
npx tsc --init
```

## Recommended formatting & linters

### ESLint + Prettier (recommended)

```bash
npm install -D eslint @typescript-eslint/parser @typescript-eslint/eslint-plugin prettier eslint-config-prettier
```

Example `eslint.config.mjs`:

```js
import tseslint from "@typescript-eslint/eslint-plugin";
import parser from "@typescript-eslint/parser";

export default [
  {
    files: ["src/**/*.ts"],
    languageOptions: { parser },
    plugins: { "@typescript-eslint": tseslint },
    rules: {
      "@typescript-eslint/no-explicit-any": "warn",
      "@typescript-eslint/explicit-function-return-type": ["warn", { allowExpressions: true }],
      "@typescript-eslint/consistent-type-imports": "error",
    },
  },
];
```

### Code style essentials

- Enable `"strict": true` in `tsconfig.json` for the full set of type-safety checks
- Prefer specific types over `any`; use `unknown` for untrusted input and narrow explicitly
- Use `interface` for extendable contracts and `type` for unions, intersections, and mapped types
- Use `readonly` properties and `ReadonlyArray<T>` to document immutability
- Use discriminated unions with exhaustive `switch` checks for robust pattern matching
- Prefer ES module `import`/`export`; avoid `namespace` and `module` keywords

```typescript
interface User {
  readonly id: string;
  name: string;
  role: "admin" | "editor" | "viewer";
}

function greetUser(user: User): string {
  return `Hello, ${user.name} (${user.role})!`;
}
```

## Testing & CI recommendations

### Vitest (recommended) or Jest

```bash
npm install -D vitest
```

Example test:

```typescript
import { describe, expect, it } from "vitest";
import { greetUser } from "../src/greet.js";

describe("greetUser", () => {
  it("returns greeting with name and role", () => {
    const user = { id: "1", name: "Alice", role: "admin" as const };
    expect(greetUser(user)).toBe("Hello, Alice (admin)!");
  });
});
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
      - run: npm ci
      - run: npx tsc --noEmit
      - run: npx eslint src/
      - run: npx vitest run --coverage
```

## Packaging & release guidance

- Emit declaration files (`declaration: true`) and consider `declarationMap` for consumer debugging
- Keep `main`, `module`, and `types` fields in `package.json` accurate
- Use `tsup` or `tsc` + rollup for consistent ESM/CJS dual builds
- Avoid shipping `tsconfig.json` or source files unless intentional
- Use `isolatedModules: true` for compatibility with SWC/esbuild transpilation

## Security & secrets best practices

- Treat all external input as untrusted — types describe shape, not safety
- Validate runtime input with libraries like [Zod](https://zod.dev/) to bridge type-time and run-time safety
- Never embed secrets in client-side code; use server-side environment variables
- Audit dependencies regularly with `npm audit`
- Enable Content Security Policy (CSP) headers for web applications

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Runtime validation | [Zod](https://zod.dev/) | Schema-first validation with TS inference |
| HTTP framework | [Fastify](https://fastify.dev/) / [Express](https://expressjs.com/) | Node.js servers |
| Full-stack types | [tRPC](https://trpc.io/) | End-to-end type-safe APIs |
| Testing | [Vitest](https://vitest.dev/) | Fast, Vite-native test runner |
| Linting | [@typescript-eslint](https://typescript-eslint.io/) | Type-aware ESLint rules |
| Build | [tsup](https://tsup.egoist.dev/) | Zero-config TS bundler |

## Minimal example

```typescript
// hello.ts
function greet(name: string = "world"): string {
  return `Hello, ${name}!`;
}

console.log(greet("TypeScript"));
```

```bash
npx tsx hello.ts
# Output: Hello, TypeScript!
```

## Further reading

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/) — official guide to language features
- [typescript-eslint documentation](https://typescript-eslint.io/) — type-aware linting rules and configuration
- [Total TypeScript](https://www.totaltypescript.com/) — advanced patterns and techniques

## Resources

- TypeScript official documentation — https://www.typescriptlang.org/docs/
- TypeScript Handbook — https://www.typescriptlang.org/docs/handbook/
- typescript-eslint — https://typescript-eslint.io/
- TSConfig reference — https://www.typescriptlang.org/tsconfig/
- Vitest documentation — https://vitest.dev/
- Node.js TypeScript support — https://nodejs.org/en/learn/getting-started/nodejs-with-typescript
- Zod schema validation — https://zod.dev/
- DefinitelyTyped — https://github.com/DefinitelyTyped/DefinitelyTyped
