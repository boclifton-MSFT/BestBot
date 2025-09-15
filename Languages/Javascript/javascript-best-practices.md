# JavaScript Best Practices

A concise, opinionated checklist for writing maintainable, modern, and performant JavaScript code. These practices are based on ECMAScript standards, community consensus, and proven patterns from the JavaScript ecosystem.

## Quick checklist

- Use modern ES6+ features: const/let, arrow functions, destructuring, template literals.
- Prefer strict mode (`'use strict'`) or use a module system (ES modules).
- Use meaningful variable and function names; avoid abbreviations and single-letter variables.
- Handle errors explicitly with try-catch blocks and proper error propagation.
- Use async/await for asynchronous code instead of nested callbacks or complex promise chains.
- Write pure functions when possible; minimize side effects and mutations.
- Use ESLint and Prettier for consistent code style and quality.
- Prefer const for values that don't change, let for variables that do, avoid var.
- Use strict equality (===) instead of loose equality (==).
- Write unit tests with a testing framework like Jest, Vitest, or Mocha.

---

## Language Fundamentals

### Variable Declarations
- **Use `const` by default** for values that won't be reassigned. This signals intent and prevents accidental reassignment.
- **Use `let` for reassignable variables** in block scope. Avoid `var` due to function-scoping and hoisting confusion.
- **Choose descriptive names** that clearly indicate the variable's purpose: `userAccountBalance` instead of `bal`.

```javascript
// Good
const apiEndpoint = 'https://api.example.com';
let currentUser = null;

// Avoid
var x = 'https://api.example.com';
var u;
```

### Functions
- **Prefer arrow functions** for short, inline functions and when you need lexical `this` binding.
- **Use function declarations** for main functions that need hoisting or when readability benefits from the function keyword.
- **Keep functions small and focused** on a single responsibility.
- **Use descriptive parameter names** and consider destructuring for object parameters.

```javascript
// Good - arrow function for callback
const numbers = [1, 2, 3].map(num => num * 2);

// Good - function declaration for main logic
function calculateTotalPrice({ items, taxRate, discount = 0 }) {
  const subtotal = items.reduce((sum, item) => sum + item.price, 0);
  const discountedPrice = subtotal * (1 - discount);
  return discountedPrice * (1 + taxRate);
}
```

### Equality and Comparisons
- **Use strict equality (`===`) and inequality (`!==`)** to avoid type coercion surprises.
- **Use `Object.is()` for special cases** like distinguishing -0 from +0 or checking for NaN.

```javascript
// Good
if (value === null) { /* handle null */ }
if (typeof value === 'string') { /* handle string */ }

// Avoid
if (value == null) { /* this matches both null and undefined */ }
```

## Modern JavaScript Features

### ES6+ Syntax
- **Use template literals** for string interpolation and multi-line strings.
- **Use destructuring** to extract values from objects and arrays cleanly.
- **Use rest/spread operators** for function parameters and array/object manipulation.
- **Use default parameters** instead of manual undefined checks.

```javascript
// Template literals
const message = `Hello, ${userName}! You have ${unreadCount} unread messages.`;

// Destructuring
const { name, email } = user;
const [first, second, ...rest] = arrayItems;

// Spread operator
const newArray = [...existingArray, newItem];
const updatedObject = { ...existingObject, newProperty: value };

// Default parameters
function greet(name = 'Guest', greeting = 'Hello') {
  return `${greeting}, ${name}!`;
}
```

### Modules
- **Use ES modules** (`import`/`export`) for code organization and dependency management.
- **Prefer named exports** for utilities and multiple exports; use default exports sparingly.
- **Use relative paths** for local modules and package names for external dependencies.

```javascript
// Good - named exports
export const formatCurrency = (amount) => { /* ... */ };
export const validateEmail = (email) => { /* ... */ };

// Good - importing
import { formatCurrency, validateEmail } from './utils.js';
import lodash from 'lodash';
```

## Asynchronous Programming

### Promises and Async/Await
- **Prefer async/await** over raw promises for better readability and error handling.
- **Handle errors with try-catch** in async functions.
- **Use Promise.all()** for concurrent operations that don't depend on each other.
- **Use Promise.allSettled()** when you need results from all promises, even if some fail.

```javascript
// Good - async/await with error handling
async function fetchUserData(userId) {
  try {
    const response = await fetch(`/api/users/${userId}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch user: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Error fetching user data:', error);
    throw error; // Re-throw if caller should handle it
  }
}

// Good - concurrent operations
async function loadDashboardData() {
  try {
    const [userData, postsData, notificationsData] = await Promise.all([
      fetchUserData(userId),
      fetchUserPosts(userId),
      fetchNotifications(userId)
    ]);
    return { userData, postsData, notificationsData };
  } catch (error) {
    console.error('Failed to load dashboard data:', error);
    throw error;
  }
}
```

### Event Handling
- **Use addEventListener** instead of inline event handlers for better separation of concerns.
- **Remove event listeners** when they're no longer needed to prevent memory leaks.
- **Use event delegation** for dynamically created elements.

```javascript
// Good - proper event handling
function setupEventListeners() {
  const button = document.getElementById('submit-btn');
  const handleClick = (event) => {
    event.preventDefault();
    // Handle click
  };
  
  button.addEventListener('click', handleClick);
  
  // Return cleanup function
  return () => button.removeEventListener('click', handleClick);
}
```

## Error Handling

### Exception Management
- **Use try-catch blocks** for operations that might fail (API calls, JSON parsing, etc.).
- **Create meaningful error messages** that help with debugging.
- **Log errors appropriately** - to console for development, to logging service for production.
- **Fail fast and explicitly** rather than silently continuing with invalid state.

```javascript
// Good - comprehensive error handling
async function processUserInput(input) {
  if (!input || typeof input !== 'string') {
    throw new TypeError('Input must be a non-empty string');
  }
  
  try {
    const parsedData = JSON.parse(input);
    const validatedData = validateUserData(parsedData);
    return await saveUserData(validatedData);
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('Invalid JSON format in user input');
    } else if (error instanceof ValidationError) {
      throw new Error(`Validation failed: ${error.message}`);
    } else {
      // Log unexpected errors
      console.error('Unexpected error processing user input:', error);
      throw new Error('Failed to process user input');
    }
  }
}
```

## Performance and Best Practices

### Memory Management
- **Avoid memory leaks** by cleaning up event listeners, timers, and references.
- **Use WeakMap and WeakSet** for metadata that shouldn't prevent garbage collection.
- **Be mindful of closures** that capture large objects unnecessarily.

### DOM Manipulation
- **Minimize DOM queries** by caching element references.
- **Batch DOM updates** to avoid layout thrashing.
- **Use DocumentFragment** for inserting multiple elements.

```javascript
// Good - efficient DOM manipulation
function updateUserList(users) {
  const userList = document.getElementById('user-list'); // Cache reference
  const fragment = document.createDocumentFragment();
  
  users.forEach(user => {
    const listItem = document.createElement('li');
    listItem.textContent = user.name;
    listItem.dataset.userId = user.id;
    fragment.appendChild(listItem);
  });
  
  userList.innerHTML = ''; // Clear existing content
  userList.appendChild(fragment); // Single DOM update
}
```

### Performance Considerations
- **Debounce or throttle** high-frequency events like scroll, resize, or input.
- **Use lazy loading** for non-critical resources.
- **Prefer built-in methods** over manual loops when performance matters.

```javascript
// Good - debounced search
function createDebouncedSearch(searchFn, delay = 300) {
  let timeoutId;
  return function(query) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => searchFn(query), delay);
  };
}

const debouncedSearch = createDebouncedSearch(performSearch);
```

## Testing and Quality

### Code Quality
- **Use ESLint** with a standard configuration (Airbnb, Standard, or Google) for consistent style.
- **Use Prettier** for automatic code formatting.
- **Write self-documenting code** with clear variable and function names.
- **Add JSDoc comments** for public APIs and complex functions.

### Testing
- **Write unit tests** for pure functions and business logic.
- **Test error conditions** and edge cases, not just happy paths.
- **Use descriptive test names** that explain what behavior is being tested.
- **Mock external dependencies** (APIs, databases) in unit tests.

```javascript
// Good - descriptive test
describe('formatCurrency', () => {
  it('should format positive numbers with dollar sign and two decimal places', () => {
    expect(formatCurrency(123.456)).toBe('$123.46');
  });
  
  it('should handle zero as valid input', () => {
    expect(formatCurrency(0)).toBe('$0.00');
  });
  
  it('should throw error for non-numeric input', () => {
    expect(() => formatCurrency('invalid')).toThrow('Input must be a number');
  });
});
```

## Security Considerations

### Input Validation and Sanitization
- **Validate all user input** on both client and server sides.
- **Sanitize data** before inserting into DOM to prevent XSS attacks.
- **Use Content Security Policy (CSP)** headers to mitigate script injection.

### Safe Coding Practices
- **Avoid `eval()` and `Function()` constructor** which can execute arbitrary code.
- **Be cautious with `innerHTML`** - prefer `textContent` or `createTextNode()` for user data.
- **Use HTTPS** for all API communications in production.

```javascript
// Good - safe DOM insertion
function displayUserMessage(message) {
  const messageElement = document.getElementById('message');
  messageElement.textContent = message; // Prevents XSS
}

// Good - input validation
function validateEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!email || typeof email !== 'string') {
    throw new Error('Email must be a non-empty string');
  }
  if (!emailRegex.test(email)) {
    throw new Error('Invalid email format');
  }
  return email.toLowerCase().trim();
}
```

## Tooling and Development

### Development Environment
- **Use a package manager** (npm, yarn, or pnpm) for dependency management.
- **Pin dependency versions** in package-lock.json or yarn.lock files.
- **Use a bundler** (Webpack, Vite, Parcel) for production builds.
- **Set up hot reloading** for faster development cycles.

### Code Organization
- **Use consistent file and folder naming** (kebab-case for files, camelCase for functions).
- **Group related functionality** into modules or folders.
- **Separate concerns** - keep business logic separate from UI logic.
- **Use a style guide** and enforce it with tooling.

## Browser Compatibility

### Modern JavaScript Support
- **Know your target browsers** and use appropriate polyfills if needed.
- **Use Babel** for transpiling modern JavaScript to older browser-compatible code.
- **Test in multiple browsers**, especially the oldest ones you need to support.
- **Use feature detection** instead of browser detection when possible.

### Progressive Enhancement
- **Start with a functional baseline** that works without JavaScript.
- **Layer on JavaScript enhancements** progressively.
- **Handle JavaScript failures gracefully** with appropriate fallbacks.

---

## References and Further Reading

- **ECMAScript Language Specification**: https://tc39.es/ecma262/
- **MDN Web Docs - JavaScript**: https://developer.mozilla.org/en-US/docs/Web/JavaScript
- **JavaScript.info**: https://javascript.info/
- **ESLint Rules**: https://eslint.org/docs/rules/
- **You Don't Know JS (book series)**: https://github.com/getify/You-Dont-Know-JS

This guide focuses on vanilla JavaScript fundamentals. For framework-specific best practices (React, Vue, Angular), consult framework-specific resources in addition to these general principles.

## Resources

- MDN JavaScript docs: https://developer.mozilla.org/en-US/docs/Web/JavaScript
- ECMAScript specification: https://tc39.es/ecma262/
- ESLint: https://eslint.org/
- JavaScript.info: https://javascript.info/