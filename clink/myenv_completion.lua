-- MyEnv Clink Argmatchers & Enhancements
-- Teaches Clink how to handle all Doskey aliases for files and git

if clink.argmatcher then
    -- File and directory completion for ls, ll, la
    clink.argmatcher("ls"):addarg(clink.filematches)
    clink.argmatcher("ll"):addarg(clink.filematches)
    clink.argmatcher("la"):addarg(clink.filematches)
    
    -- Standalone commands
    clink.argmatcher("clear")
    clink.argmatcher("croot")

    -- Git alias completions
    clink.argmatcher("gs")
    clink.argmatcher("ga"):addarg(clink.filematches)
    clink.argmatcher("gc")
    clink.argmatcher("gp")
    clink.argmatcher("gl")
end
