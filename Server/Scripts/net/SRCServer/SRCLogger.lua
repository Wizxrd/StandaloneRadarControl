local format = string.format
local osdate
if os then osdate = os.date end

local logger = {
    ["openmode"] = "a",
    ["datetime"] = "%Y-%m-%d %H:%M:%S",
    ["level"] = 6,
}

logger.enum  = {
    ["none"]    = 0,
    ["alert"]   = 1,
    ["error"]   = 2,
    ["warning"] = 3,
    ["info"]    = 4,
    ["debug"]   = 5,
    ["trace"]   = 6
}

local callbacks = {
    {["method"] = "alert",   ["enum"] = "ALERT"},
    {["method"] = "error",   ["enum"] = "ERROR"},
    {["method"] = "warning", ["enum"] = "WARNING"},
    {["method"] = "info",    ["enum"] = "INFO"},
    {["method"] = "debug",   ["enum"] = "DEBUG"},
    {["method"] = "trace",   ["enum"] = "TRACE"},
}

for i, callback in ipairs(callbacks) do
    logger[callback.method] = function(self, func, message, ...)
        if self.level < i then
            return
        end
        local logMessage = format(message, ...)
        if self.file then
            local fullMessage = format("%s %s\t%s: %s: %s\n", osdate(self.datetime), callback.enum, self.source, func, logMessage)
            self.file:write(fullMessage)
            return
        end
        log.write(self.source, log[callback.enum], format("%s | %s", func, logMessage))
    end
end

function logger:new(source, level, file, openmode, datetime)
    local self = setmetatable({}, {__index = logger})
    self.source = source
    if type(level) == "number" then self.level = level end
    if type(level) == "string" then self.level = self.enum[level] end
    if file then
        if not openmode then openmode = logger.openmode end
        self.file = assert(io.open(file, openmode))
    end
    if datetime then self.datetime = datetime end
    return self
end

function logger:setSource(source)
    self.source = source
    return self
end

function logger:setLevel(level)
    if type(level) == "number" then self.level = level end
    if type(level) == "string" then self.level = self.enum[level] end
    return self
end

function logger:setFile(file, openmode)
    if self.file then self.file:close() end
    if not openmode then openmode = logger.openmode end
    self.file = assert(io.open(file, openmode))
    return self
end

function logger:setDateTime(datetime)
    self.datetime = datetime
    return self
end

return logger