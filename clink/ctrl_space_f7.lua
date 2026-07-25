-- MyEnv Clink Lua Extension
-- Binds Ctrl+Space to win-history-list (the exact F7 boxed history menu)

clink.onbeginedit(function()
    rl.setbinding([[\C-@]], "win-history-list", "emacs")
    rl.setbinding([["Control-space"]], "win-history-list", "emacs")
    rl.setbinding([["C-Space"]], "win-history-list", "emacs")
end)
