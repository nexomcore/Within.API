namespace WithinAPI.Services;

public static class WellbeingRecommendationRules
{
    private static readonly Dictionary<string, string[]> GoalCategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["reduce_stress"] = ["meditation", "breathwork", "mindfulness", "stress_management"],
        ["build_strength"] = ["gym", "fitness", "strength"],
        ["improve_fitness"] = ["running", "walking", "cycling", "sports", "fitness"],
        ["improve_flexibility"] = ["yoga", "pilates", "mobility"],
        ["better_sleep"] = ["meditation", "sound_healing", "breathwork", "mindfulness"],
        ["meet_like_minded_people"] = ["community", "circles", "walking", "yoga"],
        ["develop_mindfulness"] = ["mindfulness", "meditation", "breathwork"],
        ["spiritual_growth"] = ["spiritual_growth", "meditation", "retreats", "conscious_living"],
        ["create_healthy_habits"] = ["habits", "walking", "mindfulness", "yoga"],
        ["lose_weight"] = ["gym", "running", "walking", "cycling"]
    };

    // Wellbeing profile data is lifestyle information, not medical diagnosis data.
    // Keep recommendations category-based and avoid clinical interpretation.
    public static string[] BuildRecommendedCategories(IEnumerable<string> interestKeys, IEnumerable<string> goalKeys)
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var interest in interestKeys.Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            categories.Add(interest.Trim().ToLowerInvariant());
        }

        foreach (var goal in goalKeys.Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            if (!GoalCategoryMap.TryGetValue(goal.Trim(), out var mapped)) continue;
            foreach (var category in mapped)
            {
                categories.Add(category);
            }
        }

        return categories.ToArray();
    }
}
