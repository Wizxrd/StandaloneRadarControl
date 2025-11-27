using Client.Models;

namespace Client.Services.Interfaces;

public interface IServerBookmarkService
{
    public List<ServerBookmark> GetBookmarks();
    public Task New(string address, string port, string password, string callsign);
    public Task Rename(string oldName, string newName);
    public Task Copy(ServerBookmark bookmark);
    public Task Delete(ServerBookmark bookmark);
    public Task Save(ServerBookmark bookmark);
}
