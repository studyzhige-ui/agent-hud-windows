# Agent HUD for Windows

A native Windows companion for local AI coding agents, inspired by the interaction idea of Agent HUD Open while using an original Windows implementation and visual system.

## Status

Early prototype. Native Windows Codex rollout discovery is implemented as a read-only provider. Codex quota/approvals, WSL and Claude Code are not implemented yet. The project has not yet been runtime-validated on a Windows machine.

## Current prototype

- Borderless, always-on-top WPF HUD.
- Compact and expanded session views.
- System tray recovery and exit controls.
- Local session aggregation contract with explicit provider capabilities.
- Read-only native Windows Codex session discovery from local rollout JSONL.
- Optional demo Codex / Claude cards when AGENT_HUD_DEMO=1.
- Windows CI build using .NET 10 LTS.
- Apple-inspired design principles adapted to Windows behavior and accessibility.

## Run locally

Prerequisites: Windows, .NET 10 SDK. Visual Studio 2026 with the .NET desktop development workload is recommended.

    dotnet restore src/AgentHud.Desktop/AgentHud.Desktop.csproj
    dotnet run --project src/AgentHud.Desktop/AgentHud.Desktop.csproj

To preview mock cards as well:

    set AGENT_HUD_DEMO=1
    dotnet run --project src/AgentHud.Desktop/AgentHud.Desktop.csproj

## Repository map

- docs/PRODUCT.md — product definition and delivery gates.
- docs/DESIGN_SYSTEM.md — Apple-inspired, Windows-native design rules.
- docs/ARCHITECTURE.md — application and provider architecture.
- src/AgentHud.Contracts — stable session/provider contracts.
- src/AgentHud.App — provider aggregation and application services.
- src/AgentHud.Desktop — WPF HUD, tray and optional demo provider.
- src/AgentHud.Providers.Codex — read-only Codex rollout discovery and lifecycle parsing.
- docs/CODEX_PROVIDER.md — current Codex data-access boundary and state semantics.

## Next milestone

Runtime-validate the native Windows Codex reader, then add session-index names and app-server quota reads. WSL discovery, Codex approvals and Claude Code follow as separate capabilities so unsupported data is never fabricated.

## Design and attribution

Apple Human Interface Guidelines and Apple Design Resources are study references only. This project does not redistribute Apple UI assets, fonts or trademarks and is not affiliated with Apple.

Agent HUD Open is an inspiration/reference project licensed under Apache-2.0. No upstream source code has been copied into this prototype. Any future source adaptation must preserve the applicable license and NOTICE obligations.
