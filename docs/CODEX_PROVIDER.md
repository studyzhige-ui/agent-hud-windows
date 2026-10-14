# Codex provider — Windows phase 1

## Scope

This provider performs **local, read-only session discovery** for native Windows Codex data. It does not call a model, send prompts, use credentials, change Codex settings, install hooks, or answer approvals.

## Data source

The provider respects CODEX_HOME and otherwise uses the current Windows user's .codex directory. It looks only under:

- sessions/**/rollout-*.jsonl
- archived_sessions/**/rollout-*.jsonl

This mirrors the local rollout source documented by Agent HUD Open for Codex Desktop / CLI, while the implementation here is original C#.

## Parsing boundary

To avoid loading large tool-output transcripts, the reader caps each observation:

- small rollouts: up to 704 KiB total;
- large rollouts: first 192 KiB plus final 512 KiB.

The head is used for session identity/workspace/title evidence. The tail is used for recent turn lifecycle evidence. Partial JSON lines are ignored and retried on a later refresh.

## State semantics

- Active: a task_started event is not followed by task_complete/turn_aborted in the observed evidence and activity is fresh (10 minutes).
- Completed: an explicit successful task_complete is observed.
- Idle: an explicit abort/failure is observed, or activity exists without a live turn.
- Unknown: evidence is missing, contradictory, or a running turn has become stale.

The provider intentionally does not treat a recently modified file alone as proof that an agent is running.

## Not yet supported

- Codex rate-limit/account reads through app-server.
- Desktop IPC thread following.
- PermissionRequest hooks and approvals.
- Browser/computer-use approvals.
- Mid-turn questions.
- WSL Codex homes.
- session_index.jsonl display names.

Those require separate implementations and Windows runtime validation.
