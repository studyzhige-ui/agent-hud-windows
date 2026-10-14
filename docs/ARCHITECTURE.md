# Architecture and engineering constraints

## Technology direction
- C# / .NET desktop client with WPF for a transparent, borderless, movable Windows HUD.
- Windows 10/11 primary; support Windows native CLI and WSL after separate validation.
- MVVM separation between presentation, application state and provider adapters.
- Avoid making UI depend on a particular agent's log format.

## Logical modules
```text
AgentHud.Desktop   WPF views, tray, accessibility, settings
      |
AgentHud.App       application services, session aggregator, notifications
      |
AgentHud.Contracts provider capabilities, event and usage types
      |
AgentHud.Providers.Codex
AgentHud.Providers.Claude
      |
Local provider APIs / logs / hooks / processes (as available)
```

## Data contract principles
- Session identity: provider + stable provider session key + source environment.
- Event timestamps in UTC, freshness metadata, deduplication key.
- State enums must include Unknown and Disconnected.
- Usage measurements must carry unit, source, time window and confidence category.
- Provider adapters advertise supported capabilities; UI respects missing capabilities.
- Polling must bound CPU / IO use, apply backoff, and avoid reparsing entire logs unnecessarily.

## Security & privacy
- No blanket administrative privileges.
- Treat local agent output and logs as untrusted input; never execute strings from logs.
- Never print authentication tokens, prompt content, personal file paths or secrets into diagnostics by default.
- Approval requests are not auto-executed. Validate provider/session/action bindings and expiration at response time.
- File paths and WSL path mapping are normalized defensively.

## Engineering quality
- Provider parsing fixtures and tests, including rotated/truncated files and stale sessions.
- Unit tests for transitions, missing data, token aggregation and duplicate events.
- Windows integration tests for DPI, multi-monitor, focus, accessibility and tray recovery.
- Distinguish unit-tested code from hardware-verified Windows behavior.
- CI build and tests before release claims.

## Licensing
Consult original project Apache-2.0 license and any NOTICE/dependency licenses prior to incorporating actual source. Design references do not grant rights to Apple icons, proprietary fonts or assets.
