# Design System — Calm, Apple-inspired, Windows-native

## References
- [Apple Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- [Apple Design Resources](https://developer.apple.com/design/resources/)
- [Designing for macOS](https://developer.apple.com/design/human-interface-guidelines/designing-for-macos/)
- [Get to know the new design system (WWDC25)](https://developer.apple.com/videos/play/wwdc2025/356/)
- [Build an AppKit app with the new design](https://developer.apple.com/videos/play/wwdc2025/310/)

These are **principles and study references**, not a license to redistribute Apple UI kits, assets, SF Symbols, fonts, or trademarks. Create original visual assets and use appropriately licensed Windows-supported fonts/icons.

## Product values
1. **Clarity before decoration:** important agent state first; no dense dashboards in the collapsed HUD.
2. **Depth via hierarchy:** restrained materials and surfaces; depth only where interactive boundaries benefit.
3. **Quiet by default:** neutral tones, sparse accent colors, gentle motion; no constant flashing or gratuitous animations.
4. **Direct manipulation:** predictable hover, click, drag and keyboard operations, reversible preferences.
5. **Native behavior:** Windows DPI scaling, accessibility, system theme, high contrast, multi-monitor placement, taskbar and window-focus conventions.
6. **Honest status:** visibly distinguish actual provider events, delayed information, estimates and unsupported capabilities.

## Design tokens — initial proposal, validate in actual UI
| Token | Light | Dark |
| --- | --- | --- |
| Canvas | #F5F6F8 | #121417 |
| Surface | #FFFFFF | #202328 |
| Surface elevated | #FFFFFF | #2A2E34 |
| Primary text | #171A1E | #F5F7FA |
| Secondary text | #626B76 | #A5ADB8 |
| Hairline border | #DDE1E7 | #3A4048 |
| Accent | #246BCE | #7EB4FF |
| Success | #21845B | #5AC99A |
| Attention | #B8751C | #F0BB69 |

These are design intentions, not promises of accessibility; verify text/state contrast in each theme and high-contrast mode.

- **Font:** Segoe UI / Segoe UI Variable where available; allow OS font substitution.
- **Corner radii:** HUD pill 22–28 px depending on size; panels 16 px; controls 10–12 px.
- **Spacing:** 4 px base scale (4, 8, 12, 16, 24).
- **Animation:** ~120–220 ms for lightweight transitions; disable/reduce when Windows accessibility setting requests it.
- **Indicators:** use text + shape + color; never color alone.

## UI information architecture
### Compact pill
Provider identity, concise activity status and attention count. Nothing else unless expanded.

### Expanded panel
Header → Active sessions → Attention needed → Usage / quota (when obtainable) → Footer shortcuts.

### Settings window
General · Appearance · Providers · Notifications · Privacy · About.

## Interaction
- Click pill: toggle panel; hover should never unexpectedly steal keyboard focus.
- Drag dedicated handle to reposition; do not interfere with window controls.
- Approval UI: agent identity + exact requested action + explicit approve/deny. Never default-approve.
- Esc closes a nonmodal expanded view; system tray gives recovery access if HUD is hidden.
- Keyboard navigation, accessible labels, visible focus and robust hit targets are required.

## Design review checklist
- Does the compact HUD tell the truth with minimum noise?
- Is the most urgent actionable status dominant, without using alarming color everywhere?
- Is every control discoverable with mouse, keyboard and screen reader?
- Do Windows scaling, themes, high-contrast mode and taskbar placements work?
- Are examples labeled as mock data and real observations attributed to their sources?
