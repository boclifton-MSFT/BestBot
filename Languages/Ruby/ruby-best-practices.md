# Ruby Best Practices

A comprehensive guide to writing maintainable, readable, and idiomatic Ruby based on the Ruby Style Guide, community conventions, and established ecosystem tooling.

## Overview

Ruby is a dynamic, object-oriented programming language designed for developer happiness and productivity. Its elegant syntax, powerful metaprogramming capabilities, and "principle of least surprise" philosophy make it a joy to write and read. Ruby powers the Rails web framework, is widely used for scripting and automation, and has a mature ecosystem of gems and tooling. With strong conventions around testing and code style, Ruby emphasizes code clarity and expressiveness.

## When to use Ruby in projects

- **Web applications**: Ruby on Rails, Sinatra, Hanami
- **APIs and microservices**: Rails API mode, Grape, Roda
- **Scripting and automation**: System administration, file processing, DevOps
- **CLI tools**: Thor, TTY toolkit, OptionParser
- **Prototyping**: Rapid development with expressive syntax
- **Infrastructure tooling**: Chef, Puppet, Vagrant, Homebrew
- **Testing and QA**: Capybara, Selenium bindings, test automation

## Tooling & ecosystem

### Core tools
- **Runtime**: CRuby (MRI), JRuby, TruffleRuby
- **Version manager**: [rbenv](https://github.com/rbenv/rbenv), [asdf](https://asdf-vm.com/), RVM
- **Package manager**: [Bundler](https://bundler.io/) with `Gemfile` and `Gemfile.lock`
- **Formatter / linter**: [RuboCop](https://rubocop.org/) (style + lint + metrics)
- **REPL**: [IRB](https://ruby-doc.org/stdlib/libdoc/irb/rdoc/IRB.html), [Pry](https://pry.github.io/)

### Project setup

```bash
gem install bundler
mkdir my_app && cd my_app
bundle init
# Edit Gemfile and run:
bundle install
```

## Recommended formatting & linters

### RuboCop (recommended)

```bash
gem install rubocop
rubocop --auto-correct
```

Example `.rubocop.yml`:

```yaml
AllCops:
  TargetRubyVersion: 3.3
  NewCops: enable

Style/StringLiterals:
  EnforcedStyle: single_quotes

Metrics/MethodLength:
  Max: 15
```

### Code style essentials (Ruby Style Guide)

- 2-space indentation, `snake_case` for methods/variables, `PascalCase` for classes/modules
- `SCREAMING_SNAKE_CASE` for constants
- Prefer single quotes for strings unless interpolation is needed
- Keep line length under 120 characters (80 preferred)
- Prefer blocks and iterators over traditional `for` loops
- Use guard clauses to reduce nesting

```ruby
class UserService
  def activate(user)
    return unless user.valid?

    user.update!(active: true)
    NotificationMailer.welcome(user).deliver_later
  end
end
```

## Testing & CI recommendations

### RSpec (recommended)

```bash
gem install rspec
rspec --init
```

Example test:

```ruby
RSpec.describe UserService do
  describe '#activate' do
    it 'activates a valid user' do
      user = build(:user, valid: true)
      service = described_class.new

      service.activate(user)

      expect(user).to be_active
    end

    it 'does nothing for an invalid user' do
      user = build(:user, valid: false)
      service = described_class.new

      service.activate(user)

      expect(user).not_to be_active
    end
  end
end
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
        ruby-version: ['3.2', '3.3']
    steps:
      - uses: actions/checkout@v4
      - uses: ruby/setup-ruby@v1
        with:
          ruby-version: ${{ matrix.ruby-version }}
          bundler-cache: true
      - run: bundle exec rubocop
      - run: bundle exec rspec
```

## Packaging & release guidance

- Build gems with `gem build myapp.gemspec` and publish with `gem push`
- Use a `.gemspec` file with accurate metadata (summary, homepage, license)
- Follow semantic versioning and document changes in a CHANGELOG
- Use `bundle exec` to ensure scripts run with correct gem versions
- Group gems in `Gemfile` by environment (development, test, production)

## Security & secrets best practices

- Never commit secrets; use environment variables or `dotenv` gem
- Be cautious with `eval`, `instance_eval`, and dynamic method dispatch
- Use `SecureRandom.hex` instead of `rand` for security-sensitive values
- Use strong parameters in Rails to whitelist permitted attributes
- Keep gems updated; run `bundle audit` to scan for known vulnerabilities
- Validate and sanitize all external input

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Web framework | [Rails](https://rubyonrails.org/) / [Sinatra](http://sinatrarb.com/) | Full-stack / lightweight |
| Testing | [RSpec](https://rspec.info/) + [FactoryBot](https://github.com/thoughtbot/factory_bot) | BDD testing with fixtures |
| HTTP client | [Faraday](https://lostisland.github.io/faraday/) / [HTTParty](https://github.com/jnunemaker/httparty) | HTTP requests |
| Background jobs | [Sidekiq](https://sidekiq.org/) | Redis-backed job processing |
| Linting | [RuboCop](https://rubocop.org/) | Style, lint, and metrics |
| Debugging | [Pry](https://pry.github.io/) / [debug](https://github.com/ruby/debug) | REPL and step debugger |

## Minimal example

```ruby
# hello.rb
def greet(name = 'world')
  "Hello, #{name}!"
end

puts greet('Ruby')
```

```bash
ruby hello.rb
# Output: Hello, Ruby!
```

## Further reading

- [Ruby Style Guide](https://rubystyle.guide/) — community-driven formatting and naming conventions
- [Practical Object-Oriented Design in Ruby (POODR)](https://www.poodr.com/) — OOP principles applied to Ruby
- [The Well-Grounded Rubyist](https://www.manning.com/books/the-well-grounded-rubyist-third-edition) — comprehensive language reference

## Resources

- Ruby official documentation — https://www.ruby-lang.org/en/documentation/
- Ruby Style Guide — https://rubystyle.guide/
- RuboCop linter — https://rubocop.org/
- RSpec testing — https://rspec.info/documentation/
- Bundler dependency management — https://bundler.io/
- RubyGems package registry — https://rubygems.org/
- OWASP Ruby on Rails Security — https://cheatsheetseries.owasp.org/cheatsheets/Ruby_on_Rails_Security_Cheat_Sheet.html
- Ruby Toolbox (gem discovery) — https://www.ruby-toolbox.com/
