namespace Daily_Tracker.Api.Contracts;

public class CreateWorkoutRequest
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public List<CreateWorkoutExerciseRequest> Exercises { get; set; } = new();
}

public class CreateWorkoutExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
