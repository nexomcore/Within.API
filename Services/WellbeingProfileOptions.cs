using WithinAPI.Models;

namespace WithinAPI.Services;

public static class WellbeingProfileOptions
{
    public static readonly WellbeingInterestOptionDto[] Interests =
    [
        new("Move", "gym", "Gym"),
        new("Move", "running", "Running"),
        new("Move", "walking", "Walking"),
        new("Move", "cycling", "Cycling"),
        new("Move", "yoga", "Yoga"),
        new("Move", "pilates", "Pilates"),
        new("Move", "sports", "Sports"),
        new("Move", "dance", "Dance"),
        new("Feel", "mental_wellbeing", "Mental wellbeing"),
        new("Feel", "stress_management", "Stress management"),
        new("Feel", "emotional_resilience", "Emotional resilience"),
        new("Feel", "mindfulness", "Mindfulness"),
        new("Seek", "meditation", "Meditation"),
        new("Seek", "spiritual_growth", "Spiritual growth"),
        new("Seek", "breathwork", "Breathwork"),
        new("Seek", "sound_healing", "Sound healing"),
        new("Seek", "retreats", "Retreats"),
        new("Seek", "conscious_living", "Conscious living")
    ];

    public static readonly WellbeingOptionDto[] Goals =
    [
        new("improve_fitness", "Improve fitness"),
        new("lose_weight", "Lose weight"),
        new("build_strength", "Build strength"),
        new("improve_flexibility", "Improve flexibility"),
        new("reduce_stress", "Reduce stress"),
        new("better_sleep", "Better sleep"),
        new("meet_like_minded_people", "Meet like-minded people"),
        new("develop_mindfulness", "Develop mindfulness"),
        new("spiritual_growth", "Spiritual growth"),
        new("create_healthy_habits", "Create healthy habits")
    ];

    public static readonly string[] ActivityLevels = ["Sedentary", "Lightly Active", "Active", "Very Active"];

    public static readonly WellbeingOptionDto[] MoodOptions =
    [
        new("Great", "Great"),
        new("Good", "Good"),
        new("Okay", "Okay"),
        new("Low", "Low"),
        new("Struggling", "Struggling")
    ];

    public static WellbeingOptionsDto ToDto() => new()
    {
        Interests = Interests
            .GroupBy(item => item.Category)
            .ToDictionary(group => group.Key, group => group.Select(item => new WellbeingOptionDto(item.Key, item.Label)).ToArray()),
        Goals = Goals,
        ActivityLevels = ActivityLevels,
        MoodOptions = MoodOptions
    };

    public static bool TryGetInterest(string key, out WellbeingInterestOptionDto option)
    {
        option = Interests.FirstOrDefault(item => item.Key.Equals(key, StringComparison.OrdinalIgnoreCase))!;
        return option is not null;
    }

    public static bool TryGetGoal(string key, out WellbeingOptionDto option)
    {
        option = Goals.FirstOrDefault(item => item.Key.Equals(key, StringComparison.OrdinalIgnoreCase))!;
        return option is not null;
    }

    public static bool IsActivityLevelAllowed(string? value) =>
        string.IsNullOrWhiteSpace(value) || ActivityLevels.Any(item => item.Equals(value, StringComparison.OrdinalIgnoreCase));
}
