package.path  = package.path..";.\\LuaSocket\\?.lua;"
package.cpath = package.cpath..";.\\LuaSocket\\?.dll;"
local socket = require("socket")
local json = loadfile(lfs.currentdir().."Scripts\\JSON.lua")()
local SRCLogger = loadfile("C:\\Users\\nicks\\Saved Games\\DCS.dcs_serverrelease\\Scripts\\net\\SRCServer\\SRCLogger.lua")()
local logger = SRCLogger:new("SRCMissionExport.lua")

logger:info("Mission Export Done", "Loading...")

local udpSendSocket = socket.udp()
udpSendSocket:settimeout(0)

local function sideEnumToString(side)
    if side == 0 then
        return "neutral"
    elseif side == 1 then
        return "red"
    elseif side == 2 then
        return "blue"
    end
end

SRC = {}

function SRC.SendToServer(msg)
    local success, err = pcall(function()
        socket.try(udpSendSocket:sendto(json:encode(msg), "127.0.0.1", 7700))
    end)
    if not success and err then
        logger:error("SendToServer", "ERROR: %s", tostring(err))
    end
end

local exportChunkMax = 100
local delayedIncrement = 0.25
function SRC.GetGlobalContacts()
    local contacts = {}
    for i = 0, 2 do
        for _, airplane in pairs(coalition.getGroups(i, Group.Category.AIRPLANE)) do
            for _, unit in pairs(airplane:getUnits()) do
                local name = unit:getName()
                if unit:isExist() and unit:isActive() and unit:getLife() > 1 then
                    if unit.getPlayerName and unit:getPlayerName() then
                        name = unit:getPlayerName()
                    end
                    if not contacts[name] then
                        contacts[name] = unit
                    end
                end
            end
        end
        for _, helicopter in pairs(coalition.getGroups(i, Group.Category.HELICOPTER)) do
            for _, unit in pairs(helicopter:getUnits()) do
                local name = unit:getName()
                if unit:isExist() and unit:isActive() and unit:getLife() > 1 then
                    if unit.getPlayerName and unit:getPlayerName() then
                        name = unit:getPlayerName()
                    end
                    if not contacts[name] then
                        contacts[name] = unit
                    end
                end
            end
        end
    end
    return contacts
end

local exportChunkMax = 100
local delayedIncrement = 0.25
local function exportInChunks(msg, exportType, logMsg)
    local iterations = math.ceil(#msg[exportType] / exportChunkMax)
    local currentChunk = 1
    local delay = 0.25
    local function exportChunk()
        logger:debug("exportChunk", "Exporting Chunk: %d", currentChunk)
        local chunk = {}
        chunk.callback = msg.callback
        chunk[exportType] = {}
        local startIndex = (currentChunk - 1) * exportChunkMax + 1
        local endIndex = math.min(currentChunk * exportChunkMax, #msg[exportType])
        logger:debug("exportChunk", "startIndex: %d | endIndex: %d", startIndex, endIndex)
        for i = startIndex, endIndex do
            table.insert(chunk[exportType], msg[exportType][i])
        end
        SRC.SendToServer(chunk)
        currentChunk = currentChunk + 1
        if currentChunk <= iterations then
            timer.scheduleFunction(exportChunk, nil, timer.getTime() + delay)
            delay = delay + delayedIncrement
        else
            logger:debug("exportChunk", "%s Exporter Chunk Completed", logMsg)
        end
    end
    logger:debug("sendInChunks", "Recursing")
    exportChunk()
end


function SRC.GlobalContactExporter()
    logger:debug("GlobalContactExporter", "Global Contact Exporter Started")
    local contacts = SRC.GetGlobalContacts()
    local msg = {}
    msg.callback = "OnGlobalContactExport"
    msg.contacts = {}
    for name, unit in pairs(contacts) do
        local velocityVec3 = unit:getVelocity()
        local airborne = unit:inAir()
        local typeName = unit:getTypeName()
        local side = sideEnumToString(unit:getCoalition())
        local point = unit:getPoint()
        local position = unit:getPosition()
        local lat, lon, alt = coord.LOtoLL(point)
        local wind = atmosphere.getWindWithTurbulence(point)
        local heading = math.atan2( position.x.z, position.x.x )
        if heading < 0 then
            heading = heading + 2 * math.pi
        end
        local isPlayer = false
        if unit:getPlayerName() then
            isPlayer = true
        end
        local contact = {
            ["name"] = name,
            ["player"] = isPlayer,
            ["side"] = side,
            ["lat"] = lat,
            ["lon"] = lon,
            ["alt"] = alt,
            ["velocity"] = {
                ["x"] = velocityVec3.x,
                ["y"] = velocityVec3.y,
                ["z"] = velocityVec3.z
            },
            ["wind"] = {
                ["x"] = wind.x,
                ["y"] = wind.y,
                ["z"] = wind.z
            },
            ["heading"] = heading,
            ["airborne"] = airborne,
            ["type"] = typeName,
        }
        msg.contacts[#msg.contacts+1] = contact
    end

    if #msg.contacts > exportChunkMax then
        logger:debug("GlobalContactExporter", "Sending In Chunks | contacts count: %d", #msg.contacts)
        exportInChunks(msg, "contacts", "Global Contact")
    else
        SRC.SendToServer(msg)
        logger:debug("GlobalContactExporter", "Global Contact Exporter Completed")
    end
end

local function globalContactScheduler(_, time)
    if true then
        SRC.GlobalContactExporter()
    end
    return time + 1
end

timer.scheduleFunction(globalContactScheduler, nil, timer.getTime() + 1)
