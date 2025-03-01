local hooks = {
    lfs.writedir().."Scripts\\net\\SRCServer\\SRCServerHook.lua"
}

for i = 1, #hooks do
    local success, err = pcall(function()
        loadfile(hooks[i])()
    end, nil)
    if not success and err then
        log.write("SRC-hook.lua", log.ERROR, err)
    end
end