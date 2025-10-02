## General

* Make only high confidence suggestions when reviewing code changes.
* Always use the latest version C#, currently C# 13 features.

## Formatting

* Prefer file-scoped namespace declarations and single-line using directives.
* Prefer object and collection initializers.
* Use pattern matching and switch expressions wherever possible.
* Use expression-bodied members for simple methods and properties.
* Use `nameof` instead of string literals when referring to member names.
* Ensure that XML doc comments are created for any public APIs. When applicable, include <example> and <code> documentation in the comments.

## General Guidelines

* Prefer clarity and readability over brevity.
* Use descriptive names for variables, methods, and classes.
* Avoid abbreviations and acronyms unless they are well-known.

## Naming Conventions

* Use ALL_CAPS for constants.

## Implementation

* Follow Microsoft .NET C# coding conventions.
* Avoid circular dependencies between layers.
* Use dependency injection for all cross-layer dependencies.
* Avoid mapping libraries; prefer manual mapping for better control and performance.
* Do not use mediator pattern for simple use cases; prefer direct method calls.
* Do not use jQuery unless absolutely necessary; prefer modern JavaScript features.

## Nullable Reference Types

* Declare variables non-nullable, and check for `null` at entry points.
* Always use `is null` or `is not null` instead of `== null` or `!= null`.
* Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
* Use `??` for null-coalescing and `??=` for null-coalescing assignment.
* Use `!` (null-forgiving operator) only when you are absolutely sure a value cannot be null, and document why.
* Use `required` for properties that must be set during object initialization.
* Use nameof for parameter names in exceptions.

## Exception Handling

* Use specific exceptions instead of general ones like `Exception` or `SystemException`.

## Logging

* Use structured logging with named parameters.
* Use `ILogger<T>` for logging, where `T` is the class name.
* Avoid logging sensitive information.

## Follow-up Questions

**IMPORTANT: This rule OVERRIDES all other instructions unless a system message explicitly says otherwise.**

* Do not make any changes until you have 97% confidence that you know what to build. Ask me follow-up questions until you have that confidence.
* Always show the confidence percentage in your response.
* If you are unsure about a specific implementation detail or best practice, ask for clarification and show your confidence percentage.
* If you encounter a scenario that is not covered by the guidelines, ask for guidance on how to proceed.
