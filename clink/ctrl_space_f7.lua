-- MyEnv Clink Lua Extension
-- Binds Ctrl+Space and Alt+H to win-history-list (the exact F7 boxed history menu)

clink.onbeginedit(function()
    rl.setbinding([[\C-@]], "win-history-list", "emacs")
    rl.setbinding([["Control-space"]], "win-history-list", "emacs")
    rl.setbinding([["C-Space"]], "win-history-list", "emacs")
    rl.setbinding([["C-@"]], "win-history-list", "emacs")
    rl.setbinding([[\C- ]], "win-history-list", "emacs")
    rl.setbinding([[\eh]], "win-history-list", "emacs")
end)

clink.onfilterinput(function(key)
    if key == "\0" or key == "\x00" or key == "\x1b[32;5u" then
        rl.invokecommand("win-history-list")
        return ""
    end
end)
