# R Best Practices

A concise, opinionated checklist for writing maintainable, reliable, and efficient R code. These are general, language-focused practices meant to complement package- or domain-specific guidance.

## Quick checklist

- Use consistent naming conventions; prefer snake_case for variables and functions.
- Keep functions small and focused; avoid deep nesting.
- Use vectorized operations instead of explicit loops when possible.
- Document functions with roxygen2 comments for clarity and reusability.
- Handle missing values (NA) explicitly and consistently.
- Use packages from CRAN or Bioconductor; pin versions for reproducibility.
- Write unit tests with testthat; test edge cases and error conditions.
- Use projects and renv for dependency management.
- Avoid global variables; pass parameters explicitly.
- Profile code before optimizing; measure performance impact.

---

## Naming and style

### Use consistent naming conventions
- **Variables and functions**: Use `snake_case` (e.g., `calculate_mean`, `user_data`)
- **Constants**: Use `UPPER_SNAKE_CASE` (e.g., `MAX_ITERATIONS`, `DEFAULT_TIMEOUT`)
- **Packages**: Use lowercase with dots if needed (e.g., `mypackage`, `data.table`)

```r
# Good
calculate_mean <- function(data_vector) {
  mean(data_vector, na.rm = TRUE)
}

# Avoid
calculateMean <- function(dataVector) {
  mean(dataVector, na.rm = TRUE)
}
```

### Follow consistent formatting
- Use 2 spaces for indentation
- Place opening braces on the same line
- Use spaces around operators and after commas
- Limit lines to 80 characters when practical

```r
# Good
if (condition) {
  result <- process_data(
    data = input_data,
    method = "robust"
  )
}

# Avoid
if(condition){
result<-process_data(data=input_data,method="robust")
}
```

## Functions and structure

### Keep functions small and focused
- Functions should do one thing well
- Aim for functions that fit on one screen
- Use descriptive names that indicate purpose

```r
# Good
validate_email <- function(email) {
  grepl("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", email)
}

clean_numeric_data <- function(data) {
  data[!is.na(data) & is.finite(data)]
}

# Avoid
process_user_input <- function(input) {
  # 50+ lines of mixed validation, cleaning, and processing
}
```

### Use vectorized operations
- Leverage R's vectorization for performance
- Avoid explicit loops when vectorized alternatives exist
- Use apply family functions for complex operations

```r
# Good
squared_values <- data^2
filtered_data <- data[data > threshold]
result <- sapply(data_list, mean, na.rm = TRUE)

# Avoid
squared_values <- numeric(length(data))
for (i in seq_along(data)) {
  squared_values[i] <- data[i]^2
}
```

## Documentation and testing

### Document functions with roxygen2
- Use roxygen2 comments for all exported functions
- Include parameter descriptions, return values, and examples
- Add @importFrom tags for package dependencies

```r
#' Calculate the mean of a numeric vector
#'
#' @param x A numeric vector
#' @param na.rm Logical, whether to remove NA values (default: TRUE)
#' @return A numeric value representing the mean
#' @examples
#' calculate_mean(c(1, 2, 3, NA))
#' @export
calculate_mean <- function(x, na.rm = TRUE) {
  mean(x, na.rm = na.rm)
}
```

### Write comprehensive tests
- Use testthat for unit testing
- Test normal cases, edge cases, and error conditions
- Aim for high test coverage of critical functions

```r
library(testthat)

test_that("calculate_mean works correctly", {
  expect_equal(calculate_mean(c(1, 2, 3)), 2)
  expect_equal(calculate_mean(c(1, 2, NA)), 1.5)
  expect_error(calculate_mean("text"))
})
```

## Data handling

### Handle missing values explicitly
- Check for and handle NA, NULL, and infinite values
- Use na.rm parameter consistently in statistical functions
- Consider using complete.cases() for row-wise completeness

```r
# Good
clean_data <- function(data) {
  data[complete.cases(data), ]
}

calculate_stats <- function(x) {
  list(
    mean = mean(x, na.rm = TRUE),
    median = median(x, na.rm = TRUE),
    count = sum(!is.na(x))
  )
}
```

### Use appropriate data structures
- Use data.frames for mixed-type tabular data
- Use matrices for numeric-only operations
- Consider data.table or tibble for large datasets
- Use lists for heterogeneous collections

```r
# Good
create_summary <- function(data) {
  data.frame(
    variable = names(data),
    mean = sapply(data, mean, na.rm = TRUE),
    missing = sapply(data, function(x) sum(is.na(x))),
    stringsAsFactors = FALSE
  )
}
```

## Dependencies and environment

### Use package management
- Create projects with dedicated package environments
- Use renv for dependency management and reproducibility
- Pin package versions in production environments
- Prefer CRAN packages over development versions

```r
# Initialize renv in new projects
renv::init()

# Install specific versions
renv::install("dplyr@1.1.0")

# Create reproducible lockfile
renv::snapshot()
```

### Import packages appropriately
- Use library() calls at the top of scripts
- Use package::function() for one-off calls
- Use @importFrom in package NAMESPACE files
- Avoid library() calls inside functions

```r
# Good - script header
library(dplyr)
library(ggplot2)

# Good - occasional use
result <- data.table::fread("large_file.csv")

# Avoid - inside functions
process_data <- function(data) {
  library(dplyr)  # Avoid this
  data %>% filter(value > 0)
}
```

## Performance and optimization

### Profile before optimizing
- Use Rprof() for performance profiling
- Identify bottlenecks with profvis package
- Measure memory usage with pryr or lobstr
- Focus optimization efforts on actual bottlenecks

```r
# Profile code execution
Rprof("profile.out")
result <- expensive_function(large_data)
Rprof(NULL)

# Analyze profile
summaryRprof("profile.out")
```

### Write efficient code
- Pre-allocate vectors and lists when size is known
- Use appropriate data structures for the task
- Consider parallel processing for independent operations
- Use compiled code (Rcpp) for computationally intensive tasks

```r
# Good - pre-allocation
results <- vector("list", length(input_list))
for (i in seq_along(input_list)) {
  results[[i]] <- process_item(input_list[[i]])
}

# Better - vectorized
results <- lapply(input_list, process_item)

# Even better - parallel
library(parallel)
results <- mclapply(input_list, process_item, mc.cores = 4)
```

## Error handling and debugging

### Handle errors gracefully
- Use tryCatch() for error handling
- Provide informative error messages
- Use stopifnot() for input validation
- Consider using assertthat package for assertions

```r
# Good error handling
safe_divide <- function(x, y) {
  stopifnot(is.numeric(x), is.numeric(y))
  
  if (any(y == 0)) {
    stop("Division by zero is not allowed")
  }
  
  tryCatch(
    x / y,
    error = function(e) {
      message("Error in division: ", e$message)
      return(NA)
    }
  )
}
```

### Use debugging tools effectively
- Use browser() for interactive debugging
- Use debug() and undebug() for function debugging
- Use traceback() to examine call stack after errors
- Use options(error = recover) for interactive error recovery

---

## Resources

- [R Manuals (CRAN)](https://cran.r-project.org/manuals.html)
- [R Documentation and Manuals](https://www.r-project.org/other-docs.html)
- [Advanced R by Hadley Wickham](https://adv-r.hadley.nz/)
- [R Packages by Hadley Wickham](https://r-pkgs.org/)
- [Google's R Style Guide](https://google.github.io/styleguide/Rguide.html)
- [Tidyverse Style Guide](https://style.tidyverse.org/)