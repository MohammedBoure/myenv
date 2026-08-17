using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace NightPad.Services;

/// <summary>
/// Manages syntax highlighting definitions for NightPad.
/// </summary>
public static class SyntaxService
{
    private static readonly Dictionary<string, IHighlightingDefinition> CustomDefinitions = new(StringComparer.OrdinalIgnoreCase);

    public static readonly string[] SupportedLanguages = new[]
    {
        "Plain Text",
        "PowerShell",
        "Python",
        "JSON",
        "YAML",
        "Markdown",
        "C#",
        "C/C++",
        "JavaScript",
        "TypeScript",
        "HTML",
        "XML",
        "CSS",
        "PHP",
        "SQL",
        "Batch",
        "INI / Config",
        "Java",
        "Rust",
        "Go"
    };

    static SyntaxService()
    {
        InitializeCustomSyntaxes();
    }

    public static IHighlightingDefinition? GetDefinition(string? languageOrExtension)
    {
        if (string.IsNullOrWhiteSpace(languageOrExtension) || languageOrExtension.Equals("Plain Text", StringComparison.OrdinalIgnoreCase))
            return null;

        string ext = languageOrExtension.StartsWith(".") ? languageOrExtension : Path.GetExtension(languageOrExtension);
        string name = languageOrExtension;

        // Check if matching extension directly
        if (!string.IsNullOrEmpty(ext))
        {
            string mappedName = GetLanguageByExtension(ext);
            if (CustomDefinitions.TryGetValue(mappedName, out var def))
                return def;

            var builtIn = HighlightingManager.Instance.GetDefinitionByExtension(ext);
            if (builtIn != null)
                return builtIn;
        }

        // Check custom definitions by name
        if (CustomDefinitions.TryGetValue(name, out var customDef))
            return customDef;

        // Check built-in definitions by name
        return HighlightingManager.Instance.GetDefinition(name);
    }

    public static string GetLanguageByExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return "Plain Text";

        string ext = extension.ToLowerInvariant();
        if (!ext.StartsWith(".")) ext = "." + ext;

        return ext switch
        {
            ".ps1" or ".psm1" or ".psd1" => "PowerShell",
            ".py" or ".pyw" => "Python",
            ".json" or ".jsonc" => "JSON",
            ".yml" or ".yaml" => "YAML",
            ".md" or ".markdown" => "Markdown",
            ".cs" => "C#",
            ".c" or ".cpp" or ".h" or ".hpp" or ".cc" => "C/C++",
            ".js" or ".jsx" or ".mjs" => "JavaScript",
            ".ts" or ".tsx" => "TypeScript",
            ".html" or ".htm" => "HTML",
            ".xml" or ".xaml" or ".svg" or ".csproj" or ".props" or ".targets" or ".config" => "XML",
            ".css" or ".scss" or ".sass" or ".less" => "CSS",
            ".php" => "PHP",
            ".sql" => "SQL",
            ".bat" or ".cmd" => "Batch",
            ".ini" or ".cfg" or ".conf" or ".env" => "INI / Config",
            ".java" => "Java",
            ".rs" => "Rust",
            ".go" => "Go",
            _ => "Plain Text"
        };
    }

    private static void InitializeCustomSyntaxes()
    {
        RegisterXshd("PowerShell", PowerShellXshd, new[] { ".ps1", ".psm1", ".psd1" });
        RegisterXshd("Python", PythonXshd, new[] { ".py", ".pyw" });
        RegisterXshd("JSON", JsonXshd, new[] { ".json", ".jsonc" });
        RegisterXshd("YAML", YamlXshd, new[] { ".yml", ".yaml" });
        RegisterXshd("SQL", SqlXshd, new[] { ".sql" });
        RegisterXshd("Markdown", MarkdownXshd, new[] { ".md", ".markdown" });
        RegisterXshd("Batch", BatchXshd, new[] { ".bat", ".cmd" });
        RegisterXshd("INI / Config", IniXshd, new[] { ".ini", ".cfg", ".conf", ".env" });
        RegisterXshd("Rust", RustXshd, new[] { ".rs" });
        RegisterXshd("Go", GoXshd, new[] { ".go" });
    }

    private static void RegisterXshd(string name, string xshdXml, string[] extensions)
    {
        try
        {
            using var reader = new StringReader(xshdXml);
            using var xmlReader = XmlReader.Create(reader);
            var xshd = HighlightingLoader.LoadXshd(xmlReader);
            var def = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            CustomDefinitions[name] = def;
            HighlightingManager.Instance.RegisterHighlighting(name, extensions, def);
        }
        catch
        {
            // Fallback if parsing fails
        }
    }

    #region XSHD Syntax Definitions

    private const string PowerShellXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""PowerShell"" extensions="".ps1;.psm1;.psd1"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Variable"" foreground=""#9CDCFE"" />
    <Color name=""Command"" foreground=""#DCDCAA"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Number"" foreground=""#B5CEA8"" />
    <Color name=""Parameter"" foreground=""#4EC9B0"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>&lt;#</Begin>
            <End>#&gt;</End>
        </Span>
        <Span color=""Comment"">
            <Begin>#</Begin>
        </Span>
        <Span color=""String"">
            <Begin>""</Begin>
            <End>""</End>
            <RuleSet>
                <Span color=""Variable"">
                    <Begin>\$</Begin>
                    <End>(?![a-zA-Z0-9_:])</End>
                </Span>
            </RuleSet>
        </Span>
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>
        <Rule color=""Variable"">
            \$[a-zA-Z0-9_:]+
        </Rule>
        <Rule color=""Parameter"">
            -[a-zA-Z0-9_]+
        </Rule>
        <Rule color=""Number"">
            \b0[xX][0-9a-fA-F]+\b|\b\d+(\.[0-9]+)?\b
        </Rule>
        <Keywords color=""Keyword"">
            <Word>if</Word>
            <Word>else</Word>
            <Word>elseif</Word>
            <Word>switch</Word>
            <Word>while</Word>
            <Word>for</Word>
            <Word>foreach</Word>
            <Word>do</Word>
            <Word>until</Word>
            <Word>break</Word>
            <Word>continue</Word>
            <Word>return</Word>
            <Word>try</Word>
            <Word>catch</Word>
            <Word>finally</Word>
            <Word>trap</Word>
            <Word>throw</Word>
            <Word>param</Word>
            <Word>function</Word>
            <Word>filter</Word>
            <Word>in</Word>
            <Word>trap</Word>
            <Word>process</Word>
            <Word>begin</Word>
            <Word>end</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string PythonXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Python"" extensions="".py;.pyw"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Keyword"" foreground=""#C586C0"" />
    <Color name=""BuiltIn"" foreground=""#569CD6"" />
    <Color name=""Number"" foreground=""#B5CEA8"" />
    <Color name=""Function"" foreground=""#DCDCAA"" />
    <Color name=""Decorator"" foreground=""#4EC9B0"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>#</Begin>
        </Span>
        <Span color=""String"">
            <Begin>""""""</Begin>
            <End>""""""</End>
        </Span>
        <Span color=""String"">
            <Begin>''''''</Begin>
            <End>''''''</End>
        </Span>
        <Span color=""String"">
            <Begin>""</Begin>
            <End>""</End>
        </Span>
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>
        <Rule color=""Decorator"">
            @[a-zA-Z0-9_.]+
        </Rule>
        <Rule color=""Function"">
            \bdef\s+([a-zA-Z0-9_]+)
        </Rule>
        <Rule color=""Number"">
            \b0[xX][0-9a-fA-F]+\b|\b\d+(\.[0-9]+)?\b
        </Rule>
        <Keywords color=""Keyword"">
            <Word>and</Word>
            <Word>as</Word>
            <Word>assert</Word>
            <Word>async</Word>
            <Word>await</Word>
            <Word>break</Word>
            <Word>class</Word>
            <Word>continue</Word>
            <Word>def</Word>
            <Word>del</Word>
            <Word>elif</Word>
            <Word>else</Word>
            <Word>except</Word>
            <Word>finally</Word>
            <Word>for</Word>
            <Word>from</Word>
            <Word>global</Word>
            <Word>if</Word>
            <Word>import</Word>
            <Word>in</Word>
            <Word>is</Word>
            <Word>lambda</Word>
            <Word>nonlocal</Word>
            <Word>not</Word>
            <Word>or</Word>
            <Word>pass</Word>
            <Word>raise</Word>
            <Word>return</Word>
            <Word>try</Word>
            <Word>while</Word>
            <Word>with</Word>
            <Word>yield</Word>
            <Word>True</Word>
            <Word>False</Word>
            <Word>None</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string JsonXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""JSON"" extensions="".json;.jsonc"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""PropertyName"" foreground=""#9CDCFE"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Number"" foreground=""#B5CEA8"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Comment"" foreground=""#6A9955"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>//</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>/\*</Begin>
            <End>\*/</End>
        </Span>
        <Span color=""PropertyName"">
            <Begin>(?=&quot;[^&quot;]*&quot;\s*:)</Begin>
            <End>:</End>
        </Span>
        <Span color=""String"">
            <Begin>&quot;</Begin>
            <End>&quot;</End>
        </Span>
        <Rule color=""Number"">
            -?\b\d+(\.[0-9]+)?([eE][+-]?[0-9]+)?\b
        </Rule>
        <Keywords color=""Keyword"">
            <Word>true</Word>
            <Word>false</Word>
            <Word>null</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string YamlXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""YAML"" extensions="".yml;.yaml"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""Key"" foreground=""#9CDCFE"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Number"" foreground=""#B5CEA8"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>#</Begin>
        </Span>
        <Span color=""String"">
            <Begin>&quot;</Begin>
            <End>&quot;</End>
        </Span>
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>
        <Rule color=""Key"">
            ^[ \t]*[a-zA-Z0-9_\-]+(?=\s*:)
        </Rule>
        <Rule color=""Number"">
            \b\d+(\.[0-9]+)?\b
        </Rule>
        <Keywords color=""Keyword"">
            <Word>true</Word>
            <Word>false</Word>
            <Word>yes</Word>
            <Word>no</Word>
            <Word>null</Word>
            <Word>on</Word>
            <Word>off</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string SqlXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""SQL"" extensions="".sql"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Function"" foreground=""#DCDCAA"" />
    <Color name=""Number"" foreground=""#B5CEA8"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>--</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>/\*</Begin>
            <End>\*/</End>
        </Span>
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>
        <Rule color=""Number"">
            \b\d+(\.[0-9]+)?\b
        </Rule>
        <Keywords color=""Keyword"">
            <Word>SELECT</Word><Word>select</Word>
            <Word>FROM</Word><Word>from</Word>
            <Word>WHERE</Word><Word>where</Word>
            <Word>INSERT</Word><Word>insert</Word>
            <Word>INTO</Word><Word>into</Word>
            <Word>VALUES</Word><Word>values</Word>
            <Word>UPDATE</Word><Word>update</Word>
            <Word>DELETE</Word><Word>delete</Word>
            <Word>JOIN</Word><Word>join</Word>
            <Word>LEFT</Word><Word>left</Word>
            <Word>RIGHT</Word><Word>right</Word>
            <Word>INNER</Word><Word>inner</Word>
            <Word>OUTER</Word><Word>outer</Word>
            <Word>GROUP</Word><Word>group</Word>
            <Word>BY</Word><Word>by</Word>
            <Word>ORDER</Word><Word>order</Word>
            <Word>HAVING</Word><Word>having</Word>
            <Word>LIMIT</Word><Word>limit</Word>
            <Word>CREATE</Word><Word>create</Word>
            <Word>TABLE</Word><Word>table</Word>
            <Word>DROP</Word><Word>drop</Word>
            <Word>ALTER</Word><Word>alter</Word>
            <Word>AND</Word><Word>and</Word>
            <Word>OR</Word><Word>or</Word>
            <Word>NOT</Word><Word>not</Word>
            <Word>NULL</Word><Word>null</Word>
            <Word>PRIMARY</Word><Word>primary</Word>
            <Word>KEY</Word><Word>key</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string MarkdownXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Markdown"" extensions="".md;.markdown"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Header"" foreground=""#569CD6"" fontStyle=""italic"" />
    <Color name=""Bold"" foreground=""#DCDCAA"" />
    <Color name=""Italic"" foreground=""#CE9178"" fontStyle=""italic"" />
    <Color name=""CodeBlock"" foreground=""#4EC9B0"" />
    <Color name=""Link"" foreground=""#9CDCFE"" />
    <Color name=""Quote"" foreground=""#6A9955"" />

    <RuleSet>
        <Span color=""CodeBlock"">
            <Begin>```</Begin>
            <End>```</End>
        </Span>
        <Span color=""CodeBlock"">
            <Begin>`</Begin>
            <End>`</End>
        </Span>
        <Rule color=""Header"">
            ^[#]{1,6}\s+.*$
        </Rule>
        <Rule color=""Quote"">
            ^&gt;\s+.*$
        </Rule>
        <Rule color=""Bold"">
            \*\*[^*]+\*\*
        </Rule>
        <Rule color=""Italic"">
            \*[^*]+\*
        </Rule>
        <Rule color=""Link"">
            \[[^\]]+\]\([^)]+\)
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

    private const string BatchXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Batch"" extensions="".bat;.cmd"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""Variable"" foreground=""#9CDCFE"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Label"" foreground=""#4EC9B0"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>::</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>REM</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>rem</Begin>
        </Span>
        <Rule color=""Variable"">
            %[a-zA-Z0-9_]+%|%~[a-zA-Z0-9_]+|\$[a-zA-Z0-9_]+
        </Rule>
        <Rule color=""Label"">
            ^:[a-zA-Z0-9_]+
        </Rule>
        <Keywords color=""Keyword"">
            <Word>echo</Word><Word>ECHO</Word>
            <Word>set</Word><Word>SET</Word>
            <Word>if</Word><Word>IF</Word>
            <Word>else</Word><Word>ELSE</Word>
            <Word>goto</Word><Word>GOTO</Word>
            <Word>call</Word><Word>CALL</Word>
            <Word>exit</Word><Word>EXIT</Word>
            <Word>for</Word><Word>FOR</Word>
            <Word>in</Word><Word>IN</Word>
            <Word>do</Word><Word>DO</Word>
            <Word>pause</Word><Word>PAUSE</Word>
            <Word>cls</Word><Word>CLS</Word>
            <Word>shift</Word><Word>SHIFT</Word>
            <Word>cd</Word><Word>CD</Word>
            <Word>md</Word><Word>MD</Word>
            <Word>rd</Word><Word>RD</Word>
            <Word>copy</Word><Word>COPY</Word>
            <Word>del</Word><Word>DEL</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string IniXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""INI / Config"" extensions="".ini;.cfg;.conf;.env"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""Section"" foreground=""#569CD6"" />
    <Color name=""Key"" foreground=""#9CDCFE"" />
    <Color name=""Value"" foreground=""#CE9178"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>#</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>;</Begin>
        </Span>
        <Rule color=""Section"">
            ^\[[^\]]+\]
        </Rule>
        <Rule color=""Key"">
            ^[a-zA-Z0-9_.\-]+(?=\s*=)
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

    private const string RustXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Rust"" extensions="".rs"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Type"" foreground=""#4EC9B0"" />
    <Color name=""Macro"" foreground=""#DCDCAA"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>//</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>/\*</Begin>
            <End>\*/</End>
        </Span>
        <Span color=""String"">
            <Begin>&quot;</Begin>
            <End>&quot;</End>
        </Span>
        <Rule color=""Macro"">
            [a-zA-Z0-9_]+!
        </Rule>
        <Keywords color=""Keyword"">
            <Word>as</Word><Word>break</Word><Word>const</Word><Word>continue</Word><Word>crate</Word>
            <Word>else</Word><Word>enum</Word><Word>extern</Word><Word>false</Word><Word>fn</Word>
            <Word>for</Word><Word>if</Word><Word>impl</Word><Word>in</Word><Word>let</Word>
            <Word>loop</Word><Word>match</Word><Word>mod</Word><Word>move</Word><Word>mut</Word>
            <Word>pub</Word><Word>ref</Word><Word>return</Word><Word>self</Word><Word>Self</Word>
            <Word>static</Word><Word>struct</Word><Word>super</Word><Word>trait</Word><Word>true</Word>
            <Word>type</Word><Word>unsafe</Word><Word>use</Word><Word>where</Word><Word>while</Word>
            <Word>async</Word><Word>await</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    private const string GoXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Go"" extensions="".go"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""#6A9955"" />
    <Color name=""String"" foreground=""#CE9178"" />
    <Color name=""Keyword"" foreground=""#569CD6"" />
    <Color name=""Type"" foreground=""#4EC9B0"" />

    <RuleSet>
        <Span color=""Comment"">
            <Begin>//</Begin>
        </Span>
        <Span color=""Comment"">
            <Begin>/\*</Begin>
            <End>\*/</End>
        </Span>
        <Span color=""String"">
            <Begin>&quot;</Begin>
            <End>&quot;</End>
        </Span>
        <Span color=""String"">
            <Begin>`</Begin>
            <End>`</End>
        </Span>
        <Keywords color=""Keyword"">
            <Word>break</Word><Word>default</Word><Word>func</Word><Word>interface</Word><Word>select</Word>
            <Word>case</Word><Word>defer</Word><Word>go</Word><Word>map</Word><Word>struct</Word>
            <Word>chan</Word><Word>else</Word><Word>goto</Word><Word>package</Word><Word>switch</Word>
            <Word>const</Word><Word>fallthrough</Word><Word>if</Word><Word>range</Word><Word>type</Word>
            <Word>continue</Word><Word>for</Word><Word>import</Word><Word>return</Word><Word>var</Word>
            <Word>true</Word><Word>false</Word><Word>nil</Word><Word>iota</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

    #endregion
}
