# NightPad Services Directory (`tools/nightpad/Services`)

Core business logic, path completion, Arabic language utilities, Markdown live preview renderer, and text transformation services for the NightPad editor.

## Files and Structure

| File | Purpose |
|---|---|
| [`SyntaxService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/SyntaxService.cs) | Manages syntax highlighting definitions for 20+ programming languages, content-based language auto-detection heuristics, and extension mappings. |
| [`MarkdownRenderService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/MarkdownRenderService.cs) | High-performance native dark-themed WPF FlowDocument renderer for real-time Markdown live side-by-side preview with embedded image rendering, click-to-open external image viewer, and Arabic RTL support. |
| [`PathCompletionService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/PathCompletionService.cs) | High-speed terminal-style keyboard path resolution, Tab auto-completion cycling, preset folder jumping (`F1`-`F4`), and auto-directory creation. |
| [`ArabicTextService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/ArabicTextService.cs) | Arabic character detection, smart bidirectional Right-to-Left (RTL) flow evaluation, and multilingual Unicode word counting. |
| [`TextTransformService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/TextTransformService.cs) | Provides text manipulations, casing conversions, line sorting/deduplication, JSON formatting/minifying, Base64/URL encoding and decoding, and timestamp insertion. |
