# Agent HUD for Windows — Product Definition

## Objective
Build a native, privacy-conscious Windows 10/11 companion for AI coding agents, inspired by [Agent HUD Open](https://github.com/jazzenchen/agent-hud-open). This is not only a usage meter: the goal is to surface agent activity, session context, usage, and supported human-in-the-loop actions in a calm desktop HUD.

## Core experience
- **Collapsed HUD:** a small, movable, always-on-top pill with active agent count, relevant state, and unobtrusive alerts.
- **Expanded HUD:** sessions grouped by provider, status, task context, usage data when available, and actions.
- **Tray-first behavior:** persistent tray icon, launch on startup (opt-in), pause monitoring, settings, and exit.
- **States:** active, idle, completed, requires attention, unknown, and disconnected. Never infer an agent is active just because a stale file exists.
- **Notifications:** notify for meaningful transitions and approval requests; support quiet hours and reduce-motion preferences.

## Provider strategy
Ship a provider capability matrix rather than pretending every provider exposes the same information.

| Capability | Definition |
| --- | --- |
| Discovery | Find supported local agent sessions |
| State | Distinguish running / waiting / finished with evidence |
| Usage | Report measured tokens / limits, including provenance |
| Deep link | Open the source workspace or terminal session |
| Approval | Present and respond to supported authorization prompts |

Start with Codex and Claude Code, validate native Windows and WSL independently, then evaluate additional providers (e.g., Antigravity, Cursor). If a capability is unavailable, display “Not available” rather than fabricating values.

## Constraints
- No cloud backend required; monitor locally.
- Minimal privilege; never read or transmit unrelated user files.
- Approval decisions require explicit user action, with clear requested operation and origin.
- Offline-first storage and documented data retention.
- Do not bundle copied Apple assets or imply Apple affiliation.
- Original project is Apache-2.0; preserve notices and inspect dependencies before importing code.

## Delivery sequence
1. Window, tray, positioning, theming, keyboard and accessibility primitives; test on Windows.
2. Agent session model, one provider integration, credible status refresh.
3. Second provider, usage measurements with source labels, diagnostics.
4. Approvals where an official supported integration exists, notifications and multi-display polish.
5. Release packaging, signing strategy and security review.

## Acceptance gates
Each feature must demonstrate observable real data (or be explicitly marked mocked), failure/permission handling, and Windows runtime verification. No Windows functionality is claimed shipped solely because source files were committed.
