# React Best Practices

A comprehensive guide for writing maintainable, performant, and accessible React applications. These practices are based on official React documentation, community standards, and proven patterns from the React ecosystem.

## Overview

React is a JavaScript library for building user interfaces with component-based architecture. React's declarative approach, virtual DOM, and extensive ecosystem make it ideal for building scalable frontend applications with reusable UI components, effective state management, and complex user interactions.

## When to use React in projects

- **Single Page Applications (SPAs)**: Building dynamic web applications with complex user interactions
- **Component-based UI**: When you need reusable, modular UI components across your application
- **Real-time applications**: Apps requiring frequent UI updates and state changes
- **Large team projects**: React's component architecture scales well with multiple developers
- **Data-driven interfaces**: Applications with complex state management and data flows

Avoid React for simple static websites, basic landing pages, or when SEO is critical without SSR setup.

## Tooling & ecosystem

### Development Environment
- **Create React App**: Zero-configuration React setup for beginners
- **Vite**: Fast build tool with excellent React support and hot module replacement
- **Next.js**: Full-stack React framework with SSR, routing, and deployment

### State Management
- **Built-in useState/useReducer**: For local component state
- **React Context**: For global state across component trees  
- **Redux Toolkit**: Predictable state container for complex applications
- **TanStack Query**: Server state management and caching

## Recommended formatting & linters

### ESLint Configuration
```json
{
  "extends": [
    "react-app",
    "react-app/jest",
    "@typescript-eslint/recommended"
  ],
  "plugins": ["react-hooks"],
  "rules": {
    "react-hooks/rules-of-hooks": "error",
    "react-hooks/exhaustive-deps": "warn",
    "react/prop-types": "off",
    "react/react-in-jsx-scope": "off"
  }
}
```

### Prettier Configuration
```json
{
  "semi": true,
  "trailingComma": "es5",
  "singleQuote": true,
  "printWidth": 80,
  "tabWidth": 2,
  "useTabs": false
}
```

### TypeScript Configuration
```json
{
  "compilerOptions": {
    "target": "es5",
    "lib": ["dom", "dom.iterable", "esnext"],
    "allowJs": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "module": "esnext",
    "moduleResolution": "node",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx"
  }
}
```

### Setup Commands
```bash
# Install ESLint and Prettier
npm install --save-dev eslint prettier eslint-config-prettier eslint-plugin-react-hooks

# TypeScript support
npm install --save-dev typescript @types/react @types/react-dom
```

## Testing & CI recommendations

### Testing Setup
```bash
# Testing libraries
npm install --save-dev @testing-library/react @testing-library/jest-dom @testing-library/user-event

# End-to-end testing
npm install --save-dev cypress @playwright/test
```

### Jest Configuration
```json
{
  "testEnvironment": "jsdom",
  "setupFilesAfterEnv": ["<rootDir>/src/setupTests.js"],
  "moduleNameMapping": {
    "\\.(css|less|scss|sass)$": "identity-obj-proxy"
  }
}
```

### CI Pipeline Example
```yaml
name: React CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      - run: npm ci
      - run: npm run lint
      - run: npm run test -- --coverage
      - run: npm run build
```

## Packaging & release guidance

### Build Configuration
```bash
# Production build
npm run build

# Analyze bundle size
npm install --save-dev webpack-bundle-analyzer
npx webpack-bundle-analyzer build/static/js/*.js
```

### Deployment Strategies
- **Static hosting**: Netlify, Vercel, GitHub Pages for client-side apps
- **Server-side rendering**: Next.js, Remix for SEO and performance
- **Environment variables**: Use `REACT_APP_` prefix for public variables

## Security & secrets best practices

### Environment Variables
- Use `REACT_APP_` prefix for public environment variables
- Never include secrets in client-side code - all variables are public
- Use server-side proxies for sensitive API calls

### XSS Prevention
- Sanitize user input with libraries like DOMPurify
- Use `dangerouslySetInnerHTML` sparingly and only with sanitized content
- Validate and escape data before rendering

### Dependency Security
```bash
# Check for vulnerabilities
npm audit
npm audit fix
```

## Recommended libraries

### UI Components
- **Material-UI (MUI)**: Comprehensive React component library
- **Ant Design**: Enterprise-grade UI components

### Styling
- **Styled-components**: CSS-in-JS with component styling
- **Tailwind CSS**: Utility-first CSS framework

### Forms & HTTP
- **React Hook Form**: Performant forms with minimal re-renders
- **Axios**: Promise-based HTTP client

## Minimal example

### Basic React Component
```jsx
// App.jsx
import React, { useState } from 'react';

function TodoApp() {
  const [todos, setTodos] = useState([]);
  const [inputValue, setInputValue] = useState('');

  const addTodo = () => {
    if (inputValue.trim()) {
      setTodos([...todos, { id: Date.now(), text: inputValue, completed: false }]);
      setInputValue('');
    }
  };

  const toggleTodo = (id) => {
    setTodos(todos.map(todo => 
      todo.id === id ? { ...todo, completed: !todo.completed } : todo
    ));
  };

  return (
    <div className="todo-app">
      <h1>Todo App</h1>
      <div>
        <input 
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="Add a todo..."
          onKeyPress={(e) => e.key === 'Enter' && addTodo()}
        />
        <button onClick={addTodo}>Add</button>
      </div>
      <ul>
        {todos.map(todo => (
          <li key={todo.id}>
            <span 
              style={{ textDecoration: todo.completed ? 'line-through' : 'none' }}
              onClick={() => toggleTodo(todo.id)}
            >
              {todo.text}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default TodoApp;
```

### Package.json Scripts
```json
{
  "scripts": {
    "start": "react-scripts start",
    "build": "react-scripts build",
    "test": "react-scripts test",
    "lint": "eslint src/**/*.{js,jsx}",
    "format": "prettier --write src/**/*.{js,jsx}"
  }
}
```

### CI Build Commands
```bash
# Install dependencies
npm ci

# Run linting
npm run lint

# Run tests
npm test -- --coverage --watchAll=false

# Build for production
npm run build
```

## Resources

- React official documentation: https://react.dev/
- React tutorial (Thinking in React): https://react.dev/learn/thinking-in-react
- React design patterns and best practices: https://www.telerik.com/blogs/react-design-patterns-best-practices
- Create React App documentation: https://create-react-app.dev/
- React Testing Library: https://testing-library.com/docs/react-testing-library/intro/
- React DevTools: https://react.dev/learn/react-developer-tools
- Next.js documentation: https://nextjs.org/docs
- ESLint React plugin: https://github.com/jsx-eslint/eslint-plugin-react
- React Security Guidelines: https://security.snyk.io/package/npm/react
- React TypeScript Cheatsheet: https://react-typescript-cheatsheet.netlify.app/