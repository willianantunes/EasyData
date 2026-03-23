---
paths: "**/*.js,**/*.ts"
---

# JavaScript/TypeScript rules

Any change to any js or ts file **must** include corresponding updates to its spec file (for example, `[file-name].spec.js` in the same directory). Tests must pass and maintain >95% code coverage.

```bash
# Install dependencies (once)
npm install

# Run JS tests with coverage
npm test

# Run a single test by name
npx jest --coverage --coverageReporters=text -t "filters items matching"
```

The spec uses `jest` + `jest-environment-jsdom`. The source file exposes global functions via a conditional CommonJS export (`if (typeof module !== 'undefined')`) so they are testable through `require()` while remaining transparent in the browser.