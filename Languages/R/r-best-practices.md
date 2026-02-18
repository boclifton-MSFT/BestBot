---
language_version: "4.5"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://cran.r-project.org/"
---

# R Best Practices

A comprehensive guide to writing maintainable, reliable, and efficient R code based on the Tidyverse Style Guide, R documentation, and community conventions.

## Overview

R is a programming language and environment designed for statistical computing, data analysis, and visualization. Its rich ecosystem of packages (CRAN, Bioconductor), built-in vectorized operations, and powerful plotting capabilities (ggplot2, base R graphics) make it the language of choice for statisticians, data scientists, and researchers. R's formula syntax, data frames, and functional programming features provide an expressive toolkit for exploratory data analysis, statistical modeling, and reproducible research.

## When to use R in projects

- **Statistical analysis**: Hypothesis testing, regression, Bayesian analysis
- **Data visualization**: Publication-quality plots with ggplot2, plotly
- **Bioinformatics**: Genomics pipelines with Bioconductor packages
- **Machine learning**: caret, tidymodels, xgboost, keras for R
- **Reproducible research**: R Markdown, Quarto for reports and papers
- **Data wrangling**: Cleaning and transforming datasets with dplyr, tidyr
- **Shiny applications**: Interactive web dashboards for data exploration

## Tooling & ecosystem

### Core tools
- **IDE**: [RStudio](https://posit.co/products/open-source/rstudio/) (Posit), VS Code with R extension
- **Package manager**: [renv](https://rstudio.github.io/renv/) for reproducible environments
- **Package registry**: [CRAN](https://cran.r-project.org/), [Bioconductor](https://www.bioconductor.org/)
- **Formatter / linter**: [styler](https://styler.r-lib.org/), [lintr](https://lintr.r-lib.org/)
- **Documentation**: [roxygen2](https://roxygen2.r-lib.org/) for function documentation

### Project setup

```r
# Initialize renv for dependency management
renv::init()

# Install packages
install.packages(c("tidyverse", "testthat"))

# Snapshot dependencies
renv::snapshot()
```

## Recommended formatting & linters

### styler + lintr (recommended)

```r
# Format files
styler::style_file("R/analysis.R")
styler::style_dir("R/")

# Lint files
lintr::lint("R/analysis.R")
lintr::lint_dir("R/")
```

Example `.lintr` configuration:

```yaml
linters:
  - line_length_linter(120)
  - assignment_linter()
  - commented_code_linter()
  - spaces_inside_linter()
```

### Code style essentials (Tidyverse Style Guide)

- Use `snake_case` for functions and variables; `PascalCase` is uncommon
- `UPPER_SNAKE_CASE` for constants
- 2-space indentation; spaces around operators and after commas
- Limit lines to 80 characters when practical
- Use `<-` for assignment (not `=` at the top level)
- Prefer vectorized operations over explicit loops

```r
calculate_summary <- function(data, group_col) {
  data |>
    dplyr::group_by(.data[[group_col]]) |>
    dplyr::summarise(
      mean_value = mean(value, na.rm = TRUE),
      n = dplyr::n(),
      .groups = "drop"
    )
}
```

## Testing & CI recommendations

### testthat

```r
# Set up testing infrastructure
usethis::use_testthat()
usethis::use_test("calculate_summary")
```

Example test:

```r
library(testthat)

test_that("calculate_summary computes correct mean", {
  data <- data.frame(
    group = c("a", "a", "b"),
    value = c(10, 20, 30)
  )
  result <- calculate_summary(data, "group")

  expect_equal(nrow(result), 2)
  expect_equal(result[result == "a"], 15)
})

test_that("calculate_summary handles NA values", {
  data <- data.frame(group = "a", value = c(1, NA, 3))
  result <- calculate_summary(data, "group")

  expect_equal(result, 2)
})
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: r-lib/actions/setup-r@v2
      - uses: r-lib/actions/setup-r-dependencies@v2
      - run: Rscript -e 'lintr::lint_dir("R/")'
      - run: Rscript -e 'testthat::test_dir("tests/testthat")'
```

## Packaging & release guidance

- Structure packages with `usethis::create_package()` for standard layout
- Document functions with roxygen2 (`@param`, `@return`, `@examples`, `@export`)
- Use `devtools::check()` (R CMD check) before submitting to CRAN
- Follow semantic versioning; maintain NEWS.md for changes
- Use `renv::snapshot()` for reproducible dependency lockfiles

## Security & secrets best practices

- Store API keys and credentials in `.Renviron` (never commit to source control)
- Validate and sanitize external inputs, especially in Shiny applications
- Use parameterized queries with DBI for database access
- Pin package versions with renv for reproducible, auditable environments
- Avoid `eval(parse(text = ...))` with untrusted input
- Review package sources before installing from non-CRAN repositories

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Data wrangling | [dplyr](https://dplyr.tidyverse.org/) / [data.table](https://rdatatable.gitlab.io/data.table/) | Tidy / high-performance |
| Visualization | [ggplot2](https://ggplot2.tidyverse.org/) | Grammar of graphics |
| Testing | [testthat](https://testthat.r-lib.org/) | BDD-style unit testing |
| Reporting | [R Markdown](https://rmarkdown.rstudio.com/) / [Quarto](https://quarto.org/) | Reproducible documents |
| Web apps | [Shiny](https://shiny.posit.co/) | Interactive dashboards |
| Modeling | [tidymodels](https://www.tidymodels.org/) | Unified modeling framework |

## Minimal example

```r
# hello.R
greet <- function(name = "world") {
  paste0("Hello, ", name, "!")
}

cat(greet("R"), "\n")
```

```bash
Rscript hello.R
# Output: Hello, R!
```

## Further reading

- [R for Data Science (2e)](https://r4ds.hadley.nz/) — comprehensive guide to data science with R and the tidyverse
- [Advanced R by Hadley Wickham](https://adv-r.hadley.nz/) — deep understanding of R's mechanisms
- [R Packages by Hadley Wickham](https://r-pkgs.org/) — how to create, document, and test R packages

## Resources

- R project documentation — https://www.r-project.org/other-docs.html
- CRAN package repository — https://cran.r-project.org/
- Tidyverse Style Guide — https://style.tidyverse.org/
- Advanced R (Hadley Wickham) — https://adv-r.hadley.nz/
- R Packages (Hadley Wickham) — https://r-pkgs.org/
- testthat testing framework — https://testthat.r-lib.org/
- Google's R Style Guide — https://google.github.io/styleguide/Rguide.html
- renv reproducible environments — https://rstudio.github.io/renv/
