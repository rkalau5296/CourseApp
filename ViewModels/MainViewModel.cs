using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CourseApp.Models;
using CourseApp.Repository; 

namespace CourseApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICourseRepository _courseRepository;

    [ObservableProperty]
    private string _selectedSection = "IWS";

    public ObservableCollection<Course> Courses { get; } = new();

    public MainViewModel(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
                
        _ = LoadCoursesAsync();
    }

    [RelayCommand]
    public async Task LoadCoursesAsync()
    {
        Courses.Clear();
        var coursesFromDb = await _courseRepository.GetCoursesBySectionAsync(SelectedSection);

        foreach (var course in coursesFromDb)
        {
            Courses.Add(course);
        }
    }
}