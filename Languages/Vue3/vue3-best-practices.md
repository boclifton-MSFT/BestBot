# Vue 3 Best Practices

A concise, opinionated checklist for writing maintainable, reliable, and performant Vue 3 applications. These practices are based on the official Vue.js documentation and style guide.

## Quick checklist

- Use multi-word component names to avoid conflicts with HTML elements.
- Define detailed prop types with validation for better documentation and error prevention.
- Always use `:key` with `v-for` to maintain component state predictably.
- Avoid combining `v-if` with `v-for` on the same element; use computed properties or template wrappers.
- Use component-scoped styling (scoped CSS, CSS modules, or class-based strategies).
- Prefer PascalCase for component names in templates and kebab-case in DOM templates.
- Keep expressions in templates simple; move complex logic to computed properties or methods.
- Follow consistent component file organization and naming conventions.
- Use the Composition API for better TypeScript support and logic reusability.
- Implement proper component lifecycle management and cleanup.

---

## Component Structure and Naming

... (document continues with the same content as the original `Resources/vue3-best-practices.md`)

## Resources

- Vue 3 Guide: https://vuejs.org/guide/introduction.html
- Vue Router: https://router.vuejs.org/
- Pinia (state management): https://pinia.vuejs.org/
- Composition API docs: https://vuejs.org/guide/extras/composition-api-faq.html
- Testing guidance (Vue Test Utils): https://vue-test-utils.vuejs.org/
- Security guidance (OWASP front-end): https://cheatsheetseries.owasp.org/cheatsheets/DOM_based_XSS_Prevention_Cheat_Sheet.html
