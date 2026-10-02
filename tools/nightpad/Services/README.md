# NightPad Services Directory (`tools/nightpad/Services`)

Core business logic, path completion, Arabic language utilities, Markdown live preview renderer, and text transformation services for the NightPad editor.

## Files and Structure

| File | Purpose |
|---|---|
| [`SyntaxService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/SyntaxService.cs) | Manages syntax highlighting definitions for 20+ programming languages, content-based language auto-detection heuristics, and extension mappings. |
| [`MarkdownRenderService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/MarkdownRenderService.cs) | High-performance native dark-themed WPF FlowDocument renderer for real-time Markdown live preview with table parsing, interactive image cards, mathematical plot embedding, LaTeX formula rendering, and Arabic RTL support. |
| [`MathPlotService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/MathPlotService.cs) | Native mathematical function graphing and Cartesian coordinate plotting engine with grid line generation, multi-curve rendering, real-time coordinate tracking, and clipboard image exporting. |
| [`MathFormulaService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/MathFormulaService.cs) | High-performance LaTeX mathematical formula parser and renderer supporting fractions, radicals, summations, integrals, Greek characters, operators, block cards, and inline formulas. |
| [`PathCompletionService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/PathCompletionService.cs) | High-speed terminal-style keyboard path resolution, Tab auto-completion cycling, preset folder jumping (`F1`-`F4`), and auto-directory creation. |
| [`ArabicTextService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/ArabicTextService.cs) | Arabic character detection, smart bidirectional Right-to-Left (RTL) flow evaluation, and multilingual Unicode word counting. |
| [`TextTransformService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/TextTransformService.cs) | Provides text manipulations, casing conversions, line sorting/deduplication, JSON formatting/minifying, Base64/URL encoding and decoding, and timestamp insertion. |
| [`QuickSymbolService.cs`](file:///C:/Users/moham/Documents/myenv/tools/nightpad/Services/QuickSymbolService.cs) | Manages persistent JSON storage and presets for quick symbols, snippets, math characters, typography, and frequent Arabic phrases. |

