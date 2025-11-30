using Client.Models;
using Common.Models;
using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI.Common.Connect;

public class BookmarkViewModel : ViewModelBase
{
    public ServerBookmark Model { get; set; }
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

    public Coalition Coalition
    {
        get => Model.Coalition;
        set
        {
            Model.Coalition = value;
            OnPropertyChanged();
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

    public BookmarkViewModel(ServerBookmark model)
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
