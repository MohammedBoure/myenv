-- MyEnv Clink Lua Extension
-- Binds Ctrl+Space and Alt+H to luafunc:trigger_win_history_list (the exact F7 boxed history menu)

function trigger_win_history_list()
    rl.invokecommand("win-history-list")
end

rl.setbinding([["\C-@"]], [["luafunc:trigger_win_history_list"]])
rl.setbinding([["Control-space"]], [["luafunc:trigger_win_history_list"]])
rl.setbinding([["C-Space"]], [["luafunc:trigger_win_history_list"]])
rl.setbinding([["\C- "]], [["luafunc:trigger_win_history_list"]])
rl.setbinding([["\eh"]], [["luafunc:trigger_win_history_list"]])
