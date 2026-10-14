# Agent HUD for Windows

A native Windows companion for local AI coding agents, inspired by the interaction idea of Agent HUD Open while using an original Windows implementation and visual system.

## Status

Early prototype. The current desktop shell uses clearly labeled demo data. Real Codex and Claude Code provider integrations are not implemented yet.

## Current prototype

- Borderless, always-on-top WPF HUD.
- Compact and expanded session views.
- System tray recovery and exit controls.
- Local session aggregation contract with explicit provider capabilities.
- Demo Codex / Claude session cards to validate information hierarchy.
- Windows CI build using .NET 10 LTS.
- Apple-inspired design principles adapted to Windows behavior and accessibility.

## Run locally

Prerequisites: Windows, .NET 10 SDK. Visual Studio 2026 with the .NET desktop development workload is recommended.

    dotnet restore src/AgentHud.Desktop/AgentHud.Desktop.csproj
    dotnet run --project src/AgentHud.Desktop/AgentHud.Desktop.csproj

## Repository map

- docs/PRODUCT.md — product definition and delivery gates.
- docs/DESIGN_SYSTEM.md — Apple-inspired, Windows-native design rules.
- docs/ARCHITECTURE.md — application and provider architecture.
- src/AgentHud.Contracts — stable session/provider contracts.
- src/AgentHud.App — provider aggregation and application services.
- src/AgentHud.Desktop — WPF HUD, tray and demo provider.

## Next milestone

Replace demo observations with a real Codex provider, including evidence-based session discovery, freshness metadata, failure handling and Windows/WSL distinction. Claude Code follows once the Codex contract has been validated.

## Design and attribution

Apple Human Interface Guidelines and Apple Design Resources are study references only. This project does not redistribute Apple UI assets, fonts or trademarks and is not affiliated with Apple.

Agent HUD Open is an inspiration/reference project licensed under Apache-2.0. No upstream source code has been copied into this prototype. Any future source adaptation must preserve the applicable license and NOTICE obligations.
