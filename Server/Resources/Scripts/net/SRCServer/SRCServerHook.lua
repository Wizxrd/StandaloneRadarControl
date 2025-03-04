package.path  = package.path..";.\\LuaSocket\\?.lua;"
package.cpath = package.cpath..";.\\LuaSocket\\?.dll;"
local socket = require("socket")
local json = loadfile(lfs.currentdir() .. "Scripts\\JSON.lua")()

local function envload(filename)
    local env = {}
    local file, err = loadfile(filename)
    local res = {false}
    if file then
        setfenv(file, env)
        res = {pcall(file)}
    end
    local msg
    if res[1] then msg = "OK" else msg = err end
    return env
end

local udpSendSocket = socket.udp()
local udpRecvSocket = socket.udp()
udpRecvSocket:setsockname("*", SRC_CONFIG.DCS_UDP_PORT)
udpRecvSocket:settimeout(0)