---
language_version: "ES2025"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://tc39.es/ecma262/"
---

# JavaScript Best Practices

A comprehensive guide to writing maintainable, modern, and performant JavaScript based on ECMAScript standards, MDN Web Docs, and widely adopted community conventions.

## Overview

JavaScript is a dynamic, multi-paradigm programming language that runs in browsers and server-side environments (Node.js, Deno, Bun). As the language of the web, JavaScript powers interactive user interfaces, server APIs, mobile apps, and developer tooling. Modern JavaScript (ES2015+) offers classes, modules, arrow functions, async/await, and destructuring — features that enable clean, expressive code. Its ubiquity and massive ecosystem make it one of the most widely used programming languages in the world.

## When to use JavaScript in projects

- **Interactive web UIs**: Client-side rendering, DOM manipulation, SPAs
- **Server-side applications**: Node.js APIs with Express, Fastify, or Hono
- **Full-stack web apps**: Next.js, Remix, Astro
- **Build tools and CLI utilities**: Custom scripts, task runners, developer tools
- **Serverless functions**: AWS Lambda, Azure Functions, Cloudflare Workers
- **Desktop apps**: Electron for cross-platform desktop applications
- **Rapid prototyping**: Quick iteration with dynamic typing and REPL

## Tooling & ecosystem

### Core tools
- **Runtimes**: Browser engines (V8, SpiderMonkey), [Node.js](https://nodejs.org/), [Deno](https://deno.com/), [Bun](https://bun.sh/)
- **Package manager**: npm, pnpm, yarn
- **Bundler**: [Vite](https://vitejs.dev/), [esbuild](https://esbuild.github.io/), Webpack
- **Linter**: [ESLint](https://eslint.org/)
- **Formatter**: [Prettier](https://prettier.io/)

### Project setup

```bash
mkdir my-app && cd my-app
npm init -y
npm install -D eslint prettier
```

## Recommended formatting & linters

### ESLint + Prettier (recommended)

```bash
npm install -D eslint prettier eslint-config-prettier
```

Example `eslint.config.mjs`:

```js
import js from "@eslint/js";

export default [
  js.configs.recommended,
  {
    rules: {
      "no-unused-vars": "warn",
      "no-undef": "error",
      eqeqeq: ["error", "always"],
      "prefer-const": "error",
    },
  },
];
```

### Code style essentials

- Use `const` by default; `let` when reassignment is needed; never `var`
- Use strict equality (`===`) instead of loose equality (`==`)
- Prefer arrow functions for callbacks; function declarations for named functions
- Use template literals for string interpolation: `Hello, ${name}!`
- Use destructuring for objects and arrays
- Use ES modules (`import`/`export`) instead of CommonJS (`require`)

```javascript
// Good
const formatUser = ({ name, email }) => `${name} <${email}>`;

// Avoid
var formatUser = function(user) {
  return user.name + ' <' + user.email + '>';
};
```

## Testing & CI recommendations

### Jest or Vitest

```bash
npm install -D vitest
```

Example test:

```javascript
import { describe, expect, it } from "vitest";
import { formatUser } from "../src/utils.js";

describe("formatUser", () => {
  it("formats name and email", () => {
    const user = { name: "Alice", email: "alice@example.com" };
    expect(formatUser(user)).toBe("Alice <alice@example.com>");
  });

  it("handles missing fields gracefully", () => {
    expect(formatUser({ name: "Bob", email: undefined })).toBe("Bob <undefined>");
  });
});
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
      - run: npm ci
      - run: npx eslint src/
      - run: npx prettier --check src/
      - run: npx vitest run --coverage
```

## Packaging & release guidance

- Use `package.json` with accurate `main`, `module`, and `exports` fields
- Prefer ES module output (`"type": "module"`) for modern consumers
- Use bundlers (Vite, esbuild) for production builds; ship minified output
- Follow semantic versioning and maintain a CHANGELOG
- Lock dependency versions via `package-lock.json` or `pnpm-lock.yaml`

## Security & secrets best practices

- Validate and sanitize all user input on both client and server
- Avoid `eval()`, `Function()` constructor, and `innerHTML` with user data
- Use `textContent` or `createTextNode()` for safe DOM insertion
- Never embed secrets in client-side code; use server-side environment variables
- Enable Content Security Policy (CSP) headers to mitigate XSS
- Audit dependencies regularly with `npm audit`

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| HTTP server | [Express](https://expressjs.com/) / [Fastify](https://fastify.dev/) | Established / performant |
| HTTP client | [fetch API](https://developer.mozilla.org/en-US/docs/Web/API/fetch) (built-in) | Browser and Node.js 18+ |
| Testing | [Vitest](https://vitest.dev/) / [Jest](https://jestjs.io/) | Fast / widely adopted |
| Validation | [Zod](https://zod.dev/) / [Joi](https://joi.dev/) | Schema validation |
| Linting | [ESLint](https://eslint.org/) | Pluggable linting |
| Utilities | [Lodash](https://lodash.com/) (tree-shakable) | General-purpose helpers |

## Minimal example

```javascript
// hello.js
function greet(name = "world") {
  return `Hello, ${name}!`;
}

console.log(greet("JavaScript"));
```

```bash
node hello.js
# Output: Hello, JavaScript!
```

## Further reading

- [JavaScript.info](https://javascript.info/) — modern JavaScript tutorial covering fundamentals to advanced topics
- [You Don't Know JS](https://github.com/getify/You-Dont-Know-JS) — deep-dive book series on JavaScript internals
- [MDN JavaScript Guide](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide) — comprehensive reference and guide

## Resources

- MDN JavaScript documentation — https://developer.mozilla.org/en-US/docs/Web/JavaScript
- ECMAScript specification — https://tc39.es/ecma262/
- ESLint documentation — https://eslint.org/
- JavaScript.info — https://javascript.info/
- Node.js documentation — https://nodejs.org/docs/latest/api/
- Jest testing framework — https://jestjs.io/docs/getting-started
- OWASP JavaScript Security — https://cheatsheetseries.owasp.org/cheatsheets/Nodejs_Security_Cheat_Sheet.html
- Prettier formatter — https://prettier.io/docs/en/
