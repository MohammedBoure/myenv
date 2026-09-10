# Zebar Configuration Directory (`zebar/`)

Configuration files, widget packs, and settings for **Zebar**, the customizable HTML/CSS/JS status bar for Windows and GlazeWM.

## Files & Structure

| File / Folder | Purpose |
|---|---|
| [`settings.json`](file:///C:/Users/moham/Documents/myenv/zebar/settings.json) | Zebar startup configuration defining active widget pack (`glzr-io.starter`) and presets. |
| [`packs/glzr-io.starter/`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter) | Custom widget pack with GlazeWM workspaces, clock, system stats, external microphone pronunciation button, non-clickable translator text frame, 3-squares control button with dropdown menu, item toggles, and Translation Focus Mode. |
| [`packs/glzr-io.starter/with-glazewm.html`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/with-glazewm.html) | Main HTML/React component rendering status bar widgets, external pressable microphone button with speaker pronunciation playback, non-clickable translator text frame, 50/50 divided layout, settings dropdown, and sentence reading Translation Mode. |
| [`packs/glzr-io.starter/styles.css`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/styles.css) | Custom styling for sharp dark theme (0px radius), standalone external microphone button, non-clickable text frame, pulsing speaking animation, and sentence banner. |
| [`errors.log`](file:///C:/Users/moham/Documents/myenv/zebar/errors.log) | Error diagnostics and runtime logs for Zebar. |
| [`.marketplace/`](file:///C:/Users/moham/Documents/myenv/zebar/.marketplace) | Marketplace package cache and installed pack manifests. |
