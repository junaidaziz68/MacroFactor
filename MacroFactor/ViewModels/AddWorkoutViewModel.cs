using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroFactor.Models;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace MacroFactor.ViewModels;

public partial class AddWorkoutViewModel : ObservableObject
{
    [ObservableProperty]
    private string exerciseName;

    [ObservableProperty]
    private bool isEditing;

    private LoggedWorkout _editingWorkout;

    [ObservableProperty]
    private string sets;

    [ObservableProperty]
    private string reps;

    [ObservableProperty]
    private string weight;

    [ObservableProperty]
    private string notes;

    public ObservableCollection<LoggedWorkout> Workouts { get; } = new();

    [RelayCommand]
    private async Task LogWorkoutAsync()
    {
        if (string.IsNullOrWhiteSpace(ExerciseName) || 
            string.IsNullOrWhiteSpace(Sets) || 
            string.IsNullOrWhiteSpace(Reps))
        {
            await Application.Current.MainPage.DisplayAlert("Missing Info", "Please provide at least an Exercise Name, Sets, and Reps.", "OK");
            return;
        }

        try
        {
            if (IsEditing && _editingWorkout != null)
            {
                _editingWorkout.ExerciseName = ExerciseName;
                _editingWorkout.Sets = int.Parse(Sets);
                _editingWorkout.Reps = int.Parse(Reps);
                _editingWorkout.Weight = string.IsNullOrWhiteSpace(Weight) ? 0 : double.Parse(Weight);
                _editingWorkout.Notes = Notes;

                var index = Workouts.IndexOf(_editingWorkout);
                Workouts[index] = _editingWorkout;

                IsEditing = false;
                _editingWorkout = null;
                await Application.Current.MainPage.DisplayAlert("Log Updated", $"{ExerciseName} has been successfully updated!", "OK");
            }
            else
            {
                var workout = new LoggedWorkout
                {
                    ExerciseName = ExerciseName,
                    Sets = int.Parse(Sets),
                    Reps = int.Parse(Reps),
                    Weight = string.IsNullOrWhiteSpace(Weight) ? 0 : double.Parse(Weight),
                    Notes = Notes
                };

                Workouts.Add(workout);
                await Application.Current.MainPage.DisplayAlert("Log Saved", $"{workout.ExerciseName} has been logged!", "OK");
            }

            // Clear the form for the next exercise
            ExerciseName = string.Empty;
            Sets = string.Empty;
            Reps = string.Empty;
            Weight = string.Empty;
            Notes = string.Empty;
        }
        catch (FormatException)
        {
            await Application.Current.MainPage.DisplayAlert("Invalid Input", "Make sure Sets, Reps, and Weight are numeric formats.", "OK");
        }
    }

    [RelayCommand]
    private void EditWorkout(LoggedWorkout workout)
    {
        if (workout == null) return;
        
        IsEditing = true;
        _editingWorkout = workout;
        
        ExerciseName = workout.ExerciseName;
        Sets = workout.Sets.ToString();
        Reps = workout.Reps.ToString();
        Weight = workout.Weight.ToString();
        Notes = workout.Notes;
    }

    [RelayCommand]
    private void DeleteWorkout(LoggedWorkout workout)
    {
        if (workout == null) return;
        Workouts.Remove(workout);
    }
}
