# Lua Best Practices

A comprehensive guide to writing idiomatic, maintainable, and efficient Lua code based on the official Lua documentation, the LuaRocks style guide, and established community conventions.

## Overview

Lua is a lightweight, high-level, dynamically typed scripting language designed for embedding, configuration, and rapid prototyping. Created at PUC-Rio in Brazil, Lua emphasizes simplicity, portability, and a small footprint. Its core philosophy is to provide a minimal but powerful set of mechanisms — tables as the universal data structure, first-class functions, coroutines, and metatables — letting developers build higher-level abstractions as needed.

Lua is widely used as an embedded scripting language in game engines (Roblox, LÖVE, World of Warcraft), networking tools (OpenResty/nginx, Wireshark), and IoT/embedded systems. Its C API makes it exceptionally easy to integrate with host applications written in C/C++.

## When to use Lua in projects

- **Game scripting and modding**: First-choice language for game logic, UI scripting, and modding APIs
- **Embedded configuration**: Lightweight config language for C/C++ applications
- **Networking and web**: High-performance request handling via OpenResty/nginx
- **IoT and embedded systems**: Minimal memory footprint suits resource-constrained devices (NodeMCU, ESP32)
- **Rapid prototyping**: Fast iteration with a simple, forgiving syntax
- **Data description and DSLs**: Tables make excellent data description formats
- **Plugin systems**: Easy to sandbox and embed as an extension language

## Tooling & ecosystem

### Core tools

- **Interpreter**: `lua` (reference PUC-Rio implementation) or `luajit` (JIT-compiled, Lua 5.1-compatible)
- **Package manager**: [LuaRocks](https://luarocks.org/) — the standard package manager for Lua modules
- **Linter**: [Luacheck](https://github.com/lunarmodules/luacheck) — static analysis and linting
- **Language server**: [Lua Language Server (LuaLS)](https://luals.github.io/) — diagnostics, completion, type checking via annotations
- **Formatter**: LuaLS built-in formatter or [StyLua](https://github.com/JohnnyMorganz/StyLua)

### Package management

```bash
# Install LuaRocks
# Linux/macOS:
wget https://luarocks.org/releases/luarocks-3.13.0.tar.gz
tar zxpf luarocks-3.13.0.tar.gz && cd luarocks-3.13.0
./configure && make && sudo make install

# Install a module
luarocks install luasocket

# Create a rockspec for your project
luarocks write_rockspec --output my_project-1.0-1.rockspec
```

## Recommended formatting & linters

### Luacheck

Install and configure Luacheck for static analysis:

```bash
luarocks install luacheck
luacheck src/
```

Create a `.luacheckrc` configuration file:

```lua
-- .luacheckrc
std = "lua54"           -- Target Lua 5.4 standard globals
max_line_length = false -- No hard line length limit

-- Ignore whitespace warnings (6xx) per LuaRocks style guide
ignore = {"6"}

-- Define project-specific globals
globals = {"my_app"}
```

### StyLua (formatter)

```bash
# Install via cargo or download binary
cargo install stylua

# Format a project
stylua src/

# Check formatting without modifying
stylua --check src/
```

Example `.stylua.toml`:

```toml
indent_type = "Spaces"
indent_width = 3
column_width = 120
quote_style = "AutoPreferDouble"
call_parentheses = "Always"
```

### Lua Language Server annotations

Use LuaLS annotations for type safety and documentation:

```lua
---@param name string The player's name
---@param score integer The player's score
---@return boolean success Whether the save succeeded
local function save_score(name, score)
    -- implementation
end
```

## Testing & CI recommendations

### Busted (test framework)

[Busted](https://lunarmodules.github.io/busted/) is the standard Lua testing framework:

```bash
luarocks install busted
busted spec/
```

Example test file (`spec/calculator_spec.lua`):

```lua
describe("calculator", function()
    local calc = require("calculator")

    it("adds two numbers", function()
        assert.are.equal(5, calc.add(2, 3))
    end)

    it("returns nil and error for invalid input", function()
        local result, err = calc.add("x", 3)
        assert.is_nil(result)
        assert.is_string(err)
    end)
end)
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        lua-version: ["5.4", "5.3", "luajit-2.1"]
    steps:
      - uses: actions/checkout@v4
      - uses: leafo/gh-actions-lua@v10
        with:
          luaVersion: ${{ matrix.lua-version }}
      - uses: leafo/gh-actions-luarocks@v4
      - run: luarocks install busted
      - run: luarocks install luacheck
      - run: luacheck src/
      - run: busted spec/
```

## Packaging & release guidance

- Write a `.rockspec` file describing your module, dependencies, and build instructions
- Upload to LuaRocks with `luarocks upload my_module-1.0-1.rockspec`
- Use semantic versioning: `major.minor-revision` (e.g., `1.2-1`)
- Keep modules self-contained; avoid side effects on `require`
- Always return a table from modules

```lua
-- my_module.lua
local my_module = {}

function my_module.greet(name)
    return "Hello, " .. name
end

return my_module
```

## Security & secrets best practices

- **Always use `local`** — undeclared variables become globals, polluting the shared environment
- **Sandbox untrusted code** — use `load()` with a restricted environment table to isolate scripts
- **Never use `loadstring` or `dofile` with user input** without sandboxing
- **Avoid `os.execute` and `io.popen` with unsanitized input** — command-injection risk
- **Use `pcall`/`xpcall` for error handling** — prevent stack traces from leaking internals
- **Validate all external data** — Lua's dynamic typing means type errors surface at runtime
- **Keep secrets out of Lua source** — use environment variables or external config files

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| HTTP client | [lua-resty-http](https://github.com/ledgetech/lua-resty-http) / [lua-requests](https://github.com/JakobGreen/lua-requests) | OpenResty / generic |
| JSON | [lua-cjson](https://github.com/openresty/lua-cjson) / [dkjson](https://github.com/LuaDist/dkjson) | Fast C / pure Lua |
| Networking | [LuaSocket](https://github.com/lunarmodules/luasocket) | TCP/UDP, HTTP, SMTP |
| File system | [LuaFileSystem](https://github.com/lunarmodules/luafilesystem) | Directory traversal, attributes |
| Testing | [Busted](https://github.com/lunarmodules/busted) | BDD-style test framework |
| OOP | [middleclass](https://github.com/kikito/middleclass) | Lightweight class system |

## Minimal example

```lua
-- hello.lua
local function greet(name)
    return string.format("Hello, %s!", name or "world")
end

print(greet("Lua"))
```

Run:

```bash
lua hello.lua
# Output: Hello, Lua!
```

## Key coding conventions (LuaRocks style guide)

- Use `local` for all variables and functions; avoid polluting the global namespace
- Use `snake_case` for variables and functions; `CamelCase` for classes
- Prefer `local function name()` over `local name = function()`
- Use `"double quotes"` for strings; single quotes when the string contains double quotes
- Always call `require()` with parentheses: `local m = require("module")`
- Return errors as `nil, "error message"` for expected failures; use `error()` for API misuse
- Document functions with LDoc-style comments (`--- @param`, `--- @return`)
- Run `luacheck` before every commit

## Further reading

- [Programming in Lua](https://www.lua.org/pil/) — the authoritative book by Lua's chief architect
- [Lua 5.4 Reference Manual](https://www.lua.org/manual/5.4/) — complete language specification
- [Lua Programming Gems](https://www.lua.org/gems/) — advanced patterns and techniques

## Resources

- Lua Official Documentation — https://www.lua.org/manual/5.4/
- LuaRocks Style Guide — https://github.com/luarocks/lua-style-guide
- LuaRocks Package Manager — https://luarocks.org/
- Luacheck Linter — https://luacheck.readthedocs.io/en/stable/
- Lua Language Server (LuaLS) — https://luals.github.io/
- StyLua Formatter — https://github.com/JohnnyMorganz/StyLua
- Busted Test Framework — https://lunarmodules.github.io/busted/
- Programming in Lua (Book) — https://www.lua.org/pil/
- Lua Users Wiki — http://lua-users.org/wiki/
