local fmt = import("fmt")

function init()
    -- DECSCUSR: 6 = steady vertical bar (line / I-beam) cursor
    fmt.Print("\027[6 q")
end

function onBufferOpen(buf)
    -- Enforce vertical line cursor across buffer switches
    fmt.Print("\027[6 q")
end
