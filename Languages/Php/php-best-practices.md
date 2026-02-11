---
language_version: "8.5"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://www.php.net/downloads"
---

# PHP Best Practices

A comprehensive guide to writing clean, secure, and maintainable PHP based on PHP-FIG PSR standards, the official PHP manual, and modern PHP development patterns.

## Overview

PHP is a widely used server-side scripting language designed for web development. Modern PHP (8.0+) features a robust type system with union types, enums, readonly properties, fibers, and named arguments — making it far more expressive and safe than earlier versions. PHP powers a significant portion of the web through frameworks like Laravel and Symfony, content management systems like WordPress, and is well-suited for building web applications, APIs, and command-line tools. Its low barrier to entry, extensive hosting support, and mature ecosystem make it a practical choice for projects of all sizes.

## When to use PHP in projects

- **Web applications**: Laravel, Symfony, Slim for server-rendered or API-driven apps
- **Content management**: WordPress, Drupal, and custom CMS solutions
- **E-commerce**: WooCommerce, Magento, Shopware
- **REST and GraphQL APIs**: Laravel, API Platform (Symfony)
- **CLI scripts and automation**: Symfony Console, custom CLI tools
- **Microservices**: Lightweight frameworks like Slim or Mezzio
- **Legacy system modernization**: Incremental PHP version upgrades

## Tooling & ecosystem

### Core tools
- **Runtime**: PHP 8.2+ (recommended; use latest stable)
- **Package manager**: [Composer](https://getcomposer.org/) with `composer.json` and `composer.lock`
- **Formatter**: [PHP CS Fixer](https://cs.symfony.com/) or [PHP_CodeSniffer](https://github.com/squizlabs/PHP_CodeSniffer)
- **Static analysis**: [PHPStan](https://phpstan.org/), [Psalm](https://psalm.dev/)
- **IDE**: PhpStorm, VS Code with Intelephense

### Project setup

```bash
composer init
composer require --dev phpunit/phpunit phpstan/phpstan
```

## Recommended formatting & linters

### PHP CS Fixer (recommended)

```bash
composer require --dev friendsofphp/php-cs-fixer
vendor/bin/php-cs-fixer fix src/
```

Example `.php-cs-fixer.php`:

```php
<?php
return (new PhpCsFixer\Config())
    ->setRules([
        '@PSR12' => true,
        'strict_types' => true,
        'array_syntax' => ['syntax' => 'short'],
        'no_unused_imports' => true,
    ])
    ->setFinder(
        PhpCsFixer\Finder::create()->in(__DIR__ . '/src')
    );
```

### Code style essentials (PSR-12)

- Declare `strict_types=1` at the top of every file
- 4-space indentation; `PascalCase` for classes, `camelCase` for methods/variables
- Keep line length under 120 characters
- Declare parameter types and return types for all functions
- Use PHP 8.1+ enums for fixed value sets; readonly properties for immutable data

```php
<?php
declare(strict_types=1);

final class UserService
{
    public function __construct(
        private readonly UserRepository ,
    ) {}

    public function findByEmail(string ): ?User
    {
        return ->users->findOneBy(['email' => ]);
    }
}
```

## Testing & CI recommendations

### PHPUnit

```bash
vendor/bin/phpunit tests/
```

Example test:

```php
<?php
declare(strict_types=1);

use PHPUnit\Framework\TestCase;

final class UserServiceTest extends TestCase
{
    public function testFindByEmailReturnsUser(): void
    {
         = ->createMock(UserRepository::class);
        ->method('findOneBy')->willReturn(new User('alice@example.com'));

         = new UserService();
         = ->findByEmail('alice@example.com');

        ->assertNotNull();
        ->assertSame('alice@example.com', ->getEmail());
    }
}
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
        php-version: ['8.2', '8.3']
    steps:
      - uses: actions/checkout@v4
      - uses: shivammathur/setup-php@v2
        with:
          php-version: ${{ matrix.php-version }}
      - run: composer install --no-progress
      - run: vendor/bin/php-cs-fixer fix --dry-run --diff
      - run: vendor/bin/phpstan analyse src/ --level=max
      - run: vendor/bin/phpunit tests/
```

## Packaging & release guidance

- Publish packages to Packagist with accurate `composer.json` metadata
- Use PSR-4 autoloading for consistent class-to-file mapping
- Follow semantic versioning and maintain a CHANGELOG
- Keep `composer.lock` in version control for applications; omit it for libraries
- Separate dev and production dependencies in `require-dev`

## Security & secrets best practices

- Validate and sanitize all input; never trust external data
- Use prepared statements (PDO) for all database queries — never concatenate user input into SQL
- Escape output with `htmlspecialchars()` to prevent XSS
- Store secrets in environment variables; use `vlucas/phpdotenv` for local development
- Use `password_hash()` and `password_verify()` for password handling
- Implement CSRF tokens for state-changing operations
- Keep PHP and dependencies updated; run `composer audit` for vulnerability scanning

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Web framework | [Laravel](https://laravel.com/) / [Symfony](https://symfony.com/) | Full-stack / component-based |
| HTTP client | [Guzzle](https://docs.guzzlephp.org/) | PSR-18 compliant HTTP |
| Testing | [PHPUnit](https://phpunit.de/) + [Mockery](https://github.com/mockery/mockery) | Unit testing and mocking |
| Static analysis | [PHPStan](https://phpstan.org/) / [Psalm](https://psalm.dev/) | Type-level bug detection |
| Logging | [Monolog](https://github.com/Seldaek/monolog) | PSR-3 compliant logging |
| Templating | [Blade](https://laravel.com/docs/blade) / [Twig](https://twig.symfony.com/) | Template engines |

## Minimal example

```php
<?php
// hello.php
declare(strict_types=1);

function greet(string  = 'world'): string
{
    return "Hello, {}!";
}

echo greet('PHP') . PHP_EOL;
```

```bash
php hello.php
# Output: Hello, PHP!
```

## Further reading

- [PHP The Right Way](https://phptherightway.com/) — community-curated modern PHP best practices
- [PSR Standards](https://www.php-fig.org/psr/) — PHP-FIG interoperability standards
- [Laracasts](https://laracasts.com/) — video tutorials for Laravel and modern PHP

## Resources

- PHP Manual — https://www.php.net/manual/en/
- PHP-FIG / PSR standards — https://www.php-fig.org/
- Composer package manager — https://getcomposer.org/
- PHPUnit testing framework — https://phpunit.de/
- PHPStan static analysis — https://phpstan.org/
- Laravel documentation — https://laravel.com/docs/
- OWASP PHP Security Cheat Sheet — https://cheatsheetseries.owasp.org/cheatsheets/PHP_Security_Cheat_Sheet.html
- Packagist (package registry) — https://packagist.org/
