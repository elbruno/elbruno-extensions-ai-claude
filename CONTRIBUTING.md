# Contributing to elbruno.Extensions.AI.Claude

Thank you for your interest in contributing to this project! This document provides guidelines for contributing.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature or bugfix
4. Make your changes
5. Run tests to ensure everything works
6. Submit a pull request

## Development Setup

### Prerequisites

- .NET SDK 8.0 or later
- Visual Studio 2022, VS Code, or Rider
- Git

### Building the Project

```bash
# Clone the repository
git clone https://github.com/elbruno/elbruno-extensions-ai-claude.git
cd elbruno-extensions-ai-claude

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Coding Guidelines

- Follow C# coding conventions
- Use nullable reference types
- Add XML documentation comments for public APIs
- Write unit tests for new functionality
- Keep changes focused and atomic

## Testing

All new features should include appropriate tests. Run tests before submitting:

```bash
dotnet test
```

## Pull Request Process

1. Update the README.md with details of changes if applicable
2. Ensure all tests pass
3. Update the version number in the project file following semantic versioning
4. Your PR will be reviewed by maintainers
5. Once approved, your changes will be merged

## Code Review

All submissions require review. We use GitHub pull requests for this purpose.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
