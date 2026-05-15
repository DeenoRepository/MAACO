# MAACO Known Limitations (MVP)

## Scope limitations

- Single-user local workflow is the primary supported mode.
- Distributed multi-agent execution is not implemented in MVP.
- RBAC, cloud sync, enterprise infra, and multi-user collaboration are out of scope.

## Runtime limitations

- Full publish/build for Avalonia may fail in restricted environments due to telemetry log file permissions.
- Local machine prerequisites (SDK, git, Docker where applicable) are required and not auto-provisioned.
- Some operations rely on external provider/network availability when non-Fake LLM providers are selected.

## Security and operations

- Auto-push/auto-deploy features are intentionally not enabled as MVP non-goals.
- Secrets must be provided by user configuration and are not stored in public docs/templates.
- Human-in-the-loop approval is required for final code-application decisions in approval paths.

## Testing boundaries

- Automated tests validate core workflow and reliability paths, but full environment parity still depends on host setup.
- Manual QA scenarios remain required for end-to-end validation of UI and provider integration.
