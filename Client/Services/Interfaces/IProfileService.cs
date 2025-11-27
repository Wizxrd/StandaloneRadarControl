using Client.Models;
namespace Client.Services.Interfaces
{
    public interface IProfileService
    {
        public Task<List<Profile>> GetProfiles();
        public Task New(string displayType, string position, string name);
        public Task<bool> Import();
        public Task Rename(string oldName, string newName);
        public Task Copy(Profile profile);
        public void Export(Profile profile);
        public Task Delete(Profile profile);
        public void Save(Profile profile);
        public Task SaveAsync(Profile profile);
        public Task SaveAs(Profile profile, string name);
    }
}
