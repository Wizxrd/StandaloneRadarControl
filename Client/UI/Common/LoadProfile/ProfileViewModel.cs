using Client.Models;
using Common.Mvvm;
namespace Client.UI.Common.LoadProfile;

public class ProfileViewModel : ViewModelBase
{
    public Profile Model { get; set; }
    private string originalName = string.Empty;
    private bool isSelected;
    private bool isRenaming;
    public string OriginalName => originalName;

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsSelected
    {
        get => isSelected;
        set { isSelected = value; OnPropertyChanged(); }
    }

    public bool IsRenaming
    {
        get => isRenaming;
        set { isRenaming = value; OnPropertyChanged(); }
    }

    public ProfileViewModel(Profile model)
    {
        Model = model;
        IsRenaming = false;
    }

    public void BeginRename()
    {
        originalName = Name;
        IsRenaming = true;
    }
}
