using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CourseApp.Models;
using CourseApp.Repository;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CourseApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICourseRepository _courseRepository;

    [ObservableProperty]
    private CourseNode? _selectedCourse;

    [ObservableProperty]
    private CourseNode? _selectedNode;

    [ObservableProperty]
    private CourseItem? _selectedItem;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CourseNode> Courses { get; } = new();

    // Dzieci wybranego kursu (podkursy), wyświetlane w TreeView.
    public ObservableCollection<CourseNode> CourseChildren { get; } = new();

    public ObservableCollection<CourseItem> CurrentItems { get; } = new();

    public MainViewModel(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    [RelayCommand]
    public async Task LoadCoursesAsync()
    {
        try
        {
            ErrorMessage = null;
            Courses.Clear();
            foreach (var course in await _courseRepository.GetRootCoursesAsync())
            {
                Courses.Add(course);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się wczytać kursów: {ex.Message}";
        }
    }

    async partial void OnSelectedCourseChanged(CourseNode? value)
    {
        CourseChildren.Clear();
        SelectedNode = null;
        if (value is null) return;

        try
        {
            var tree = await _courseRepository.GetCourseTreeAsync(value.CourseNodeId);
            if (tree is null || tree.CourseNodeId != SelectedCourse?.CourseNodeId) return;

            foreach (var child in tree.Children)
            {
                CourseChildren.Add(child);
            }

            // Kurs bez podkursów (np. Alchemia mocy) od razu pokazuje swoje pliki.
            if (tree.Children.Count == 0)
            {
                SelectedNode = tree;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się wczytać kursu: {ex.Message}";
        }
    }

    partial void OnSelectedNodeChanged(CourseNode? value)
    {
        CurrentItems.Clear();
        SelectedItem = null;
        if (value is null) return;

        foreach (var item in value.Items)
        {
            CurrentItems.Add(item);
        }
    }
}
