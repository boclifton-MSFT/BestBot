---
language_version: "3.5"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://github.com/vuejs/core/releases"
---

# Vue 3 Best Practices

A comprehensive guide to building maintainable, performant, and scalable Vue 3 applications using the Composition API, single-file components, and the modern Vue ecosystem.

## Overview

Vue 3 is a progressive JavaScript framework for building user interfaces. It features the Composition API for flexible logic composition, a reactivity system built on JavaScript Proxies, and first-class TypeScript support. Vue's single-file component (SFC) model co-locates template, script, and styles in `.vue` files, enabling clear component boundaries and scoped styling. With its gentle learning curve and incrementally adoptable architecture, Vue 3 is well-suited for everything from small interactive widgets to large-scale SPAs.

## When to use Vue 3 in projects

- **Single-page applications (SPAs)**: Client-rendered apps with Vue Router and Pinia
- **Server-side rendered apps**: Universal rendering with Nuxt 3
- **Progressive enhancement**: Incrementally adding interactivity to server-rendered HTML
- **Component libraries**: Reusable UI component systems with design tokens
- **Admin dashboards and internal tools**: Data-heavy CRUD interfaces
- **Static sites with dynamic features**: Content sites using VitePress or Nuxt Content
- **Micro-frontends**: Encapsulated Vue widgets embedded in larger applications

## Tooling & ecosystem

### Core tools
- **Build tool**: [Vite](https://vitejs.dev/) (official, default scaffold via `create-vue`)
- **Routing**: [Vue Router 4](https://router.vuejs.org/)
- **State management**: [Pinia](https://pinia.vuejs.org/) (official, replaces Vuex)
- **SSR / meta-framework**: [Nuxt 3](https://nuxt.com/)
- **DevTools**: [Vue DevTools](https://devtools.vuejs.org/) (browser extension)

### Project scaffolding

```bash
npm create vue@latest my-app
cd my-app
npm install
npm run dev
```

## Recommended formatting & linters

### ESLint + Prettier (recommended)

```bash
npm install -D eslint eslint-plugin-vue @vue/eslint-config-prettier prettier
```

Example `.eslintrc.cjs`:

```js
module.exports = {
  root: true,
  extends: [
    "plugin:vue/vue3-recommended",
    "@vue/eslint-config-prettier",
  ],
  rules: {
    "vue/multi-word-component-names": "error",
    "vue/no-unused-vars": "warn",
  },
};
```

### Code style essentials (Vue Style Guide)

- Use multi-word component names to avoid conflicts with HTML elements
- Use PascalCase for component names in `<script>` and templates; kebab-case in DOM templates
- Always use `:key` with `v-for`; never combine `v-if` and `v-for` on the same element
- Define props with detailed types and validators
- Keep template expressions simple; extract complex logic to computed properties
- Use `<script setup>` syntax for Composition API single-file components

```vue
<script setup lang="ts">
import { computed, ref } from "vue";

const props = defineProps<{
  title: string;
  count?: number;
}>();

const doubled = computed(() => (props.count ?? 0) * 2);
</script>
```

## Testing & CI recommendations

### Vitest + Vue Test Utils

```bash
npm install -D vitest @vue/test-utils @testing-library/vue jsdom
```

Example test:

```ts
import { mount } from "@vue/test-utils";
import { describe, expect, it } from "vitest";
import MyButton from "../MyButton.vue";

describe("MyButton", () => {
  it("renders slot content", () => {
    const wrapper = mount(MyButton, { slots: { default: "Click me" } });
    expect(wrapper.text()).toContain("Click me");
  });

  it("emits click event", async () => {
    const wrapper = mount(MyButton);
    await wrapper.trigger("click");
    expect(wrapper.emitted("click")).toHaveLength(1);
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
      - run: npm run lint
      - run: npm run type-check
      - run: npm run test -- --coverage
      - run: npm run build
```

## Packaging & release guidance

- Publish reusable components as npm packages with both ESM and CJS builds
- Use `vite-plugin-dts` to generate TypeScript declaration files for library mode
- Export components via a named entry point (`index.ts`) and document props with JSDoc or Storybook
- Follow semantic versioning; maintain a CHANGELOG

## Security & secrets best practices

- Never bind user input to `v-html`; sanitize dynamic HTML with DOMPurify
- Use environment variables (`import.meta.env.VITE_*`) for public config; never embed secrets in client code
- Enable Content Security Policy (CSP) headers to prevent XSS
- Validate all user input on the server; client-side validation is for UX, not security
- Audit dependencies regularly with `npm audit`

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| State management | [Pinia](https://pinia.vuejs.org/) | Official, type-safe stores |
| Routing | [Vue Router 4](https://router.vuejs.org/) | Official SPA routing |
| HTTP client | [Axios](https://axios-http.com/) / `fetch` | API communication |
| UI framework | [PrimeVue](https://primevue.org/) / [Vuetify 3](https://vuetifyjs.com/) | Component libraries |
| Forms | [VeeValidate](https://vee-validate.logaretm.com/) | Form validation with Composition API |
| Utilities | [VueUse](https://vueuse.org/) | Collection of Composition API utilities |

## Minimal example

```vue
<!-- App.vue -->
<script setup lang="ts">
import { ref } from "vue";

const name = ref("Vue 3");
</script>

<template>
  <h1>Hello, {{ name }}!</h1>
</template>
```

```bash
npm create vue@latest hello-vue && cd hello-vue
npm install && npm run dev
# Opens at http://localhost:5173
```

## Further reading

- [Vue 3 Composition API FAQ](https://vuejs.org/guide/extras/composition-api-faq.html) — why and when to use the Composition API
- [Vue Style Guide](https://vuejs.org/style-guide/) — official priority-based style recommendations
- [Nuxt 3 documentation](https://nuxt.com/docs) — SSR and full-stack Vue framework

## Resources

- Vue 3 official guide — https://vuejs.org/guide/introduction.html
- Vue Router documentation — https://router.vuejs.org/
- Pinia state management — https://pinia.vuejs.org/
- Composition API docs — https://vuejs.org/guide/extras/composition-api-faq.html
- Vue Test Utils — https://test-utils.vuejs.org/
- Vue Style Guide — https://vuejs.org/style-guide/
- Vite build tool — https://vitejs.dev/
- VueUse composables — https://vueuse.org/
- Security guidance (OWASP front-end): https://cheatsheetseries.owasp.org/cheatsheets/DOM_based_XSS_Prevention_Cheat_Sheet.html
