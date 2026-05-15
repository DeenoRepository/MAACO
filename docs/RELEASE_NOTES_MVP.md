# MAACO MVP Release Notes

## Version

- MVP pre-release (Milestone 23 hardening phase)

## Included

- Core domain, workflow statuses, and deterministic orchestration contracts
- Persistence layer with EF Core + SQLite and migration support
- API endpoints for workflow lifecycle, approval actions, and diagnostics
- Avalonia desktop UI for projects, tasks, workflow monitoring, and diff review
- Tooling layer for scan, patch, build/test, git, and logs
- Sandbox execution layer with path and command safety restrictions
- LLM provider abstraction with Fake, OpenAI-compatible, and Ollama flows
- Reliability and recovery behavior validated in prior milestones
- Updated operational documentation in README and milestone checklist

## Notes

- This release is focused on local single-user MVP operation.
- Human approval is expected in approval workflow paths.
- See known constraints in `docs/KNOWN_LIMITATIONS.md`.
