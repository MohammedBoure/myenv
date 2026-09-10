# Starter Widget Pack (`zebar/packs/glzr-io.starter/`)

A customized widget pack for Zebar integrated with GlazeWM.

## Files & Structure

| File | Purpose |
|---|---|
| [`with-glazewm.html`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/with-glazewm.html) | Main HTML/React component rendering GlazeWM workspaces, clock, system monitors, 3-squares control button, settings dropdown, and translator widget with interactive microphone icon and speaker pronunciation. |
| [`styles.css`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/styles.css) | Custom stylesheet with Obsidian Sharp Dark styling (0px border radius), interactive microphone button, pulsing speaking animation, and layout rules. |
| [`vanilla.html`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/vanilla.html) | Standalone vanilla bar template without window manager bindings. |
| [`with-komorebi.html`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/with-komorebi.html) | Status bar template configured for the Komorebi tiling window manager. |
| [`package.json`](file:///C:/Users/moham/Documents/myenv/zebar/packs/glzr-io.starter/package.json) | Pack manifest defining widget pack metadata, entry points, and schema version. |

## Translator Widget Features

- **Microphone Icon**: Displays a microphone icon (`nf-md-microphone`) replacing the previous globe icon.
- **Audio Pronunciation Playback**: Clicking the microphone icon switches the icon to a speaker (`nf-md-volume_high`) with pulsing animation while pronouncing the active English text using natural Google Text-to-Speech, with automatic fallback to Web Speech API and local daemon synthesis.
- **Divided Layout**: 50/50 symmetric space allocation for English text and Arabic translation.
- **Dropdown Controls**: Tabbed menu for toggling auto-capture, clipboard translation, bilingual display, focus mode, and bar container visibility.
