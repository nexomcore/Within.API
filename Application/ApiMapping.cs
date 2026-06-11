using System.Security.Claims;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Application;

public static class ApiMapping
{
    private static readonly HashSet<string> IntensityOptions = new(StringComparer.OrdinalIgnoreCase) { "low", "medium", "high" };
    private static readonly HashSet<string> ExperienceLevelOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "beginner_friendly",
        "some_experience_recommended",
        "experienced_participants_only"
    };
    private static readonly HashSet<string> AgeRestrictionOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "all_ages",
        "13_plus",
        "16_plus",
        "18_plus",
        "seniors_focused",
        "family_kids_friendly"
    };

    public const string RetreatEventType = "retreat";
    private const string DefaultEventType = "class";
    private static readonly HashSet<string> EventTypeOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "class",
        "workshop",
        "meetup",
        "meditation",
        "yoga",
        "fitness",
        "sound_healing",
        "hiking",
        "retreat",
        "festival",
        "other"
    };
    private static readonly HashSet<string> RetreatFocusOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "meditation",
        "yoga",
        "spiritual",
        "wellness",
        "fitness",
        "detox",
        "mens_retreat",
        "womens_retreat",
        "corporate_wellness",
        "nature",
        "mindfulness"
    };
    private static readonly HashSet<string> DifficultyLevelOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "beginner",
        "intermediate",
        "advanced",
        "all_levels"
    };
    private static readonly HashSet<string> FacilitiesAvailableOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "parking",
        "wheelchair_access",
        "showers",
        "wifi",
        "vegan_meals",
        "vegetarian_meals",
        "private_rooms",
        "shared_rooms",
        "accessible_toilets",
        "first_aid"
    };

    public static Guid UserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User id claim missing."));

    public static Guid? TryUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public static UserSummaryDto ToDto(this User user) => new(user.Id, user.DisplayName, user.Email, user.RoleEnum, user.PreferredLens);

    public static DailyCheckInDto ToDto(this DailyCheckIn checkIn) => new()
    {
        Id = checkIn.Id.ToString(),
        CheckInDate = checkIn.CheckInDate.ToString("yyyy-MM-dd"),
        Mood = checkIn.Mood.ToString(),
        Energy = checkIn.Energy.ToString(),
        SleepQuality = checkIn.SleepQuality?.ToString(),
        SleepHours = checkIn.SleepHours,
        Intention = checkIn.Intention.ToString(),
        Tags = checkIn.Tags,
        Note = checkIn.Note,
        SuggestedActionKey = checkIn.SuggestedActionKey,
        SuggestedAction = SuggestedActionRules.TextForKey(checkIn.SuggestedActionKey),
        DailyBalanceScore = checkIn.DailyBalanceScore
    };

    public static WellbeingProfileDto ToDto(this UserWellbeingProfile profile) => new()
    {
        Id = profile.Id.ToString(),
        UserId = profile.UserId.ToString(),
        FirstName = profile.FirstName,
        DisplayName = profile.DisplayName,
        UsePseudonym = profile.UsePseudonym,
        Pseudonym = profile.Pseudonym,
        DateOfBirth = profile.DateOfBirth?.ToString("yyyy-MM-dd"),
        Age = profile.Age,
        AgeRange = profile.AgeRange,
        Gender = profile.Gender,
        LocationCountry = profile.LocationCountry,
        LocationCity = profile.LocationCity,
        LocationSuburb = profile.LocationSuburb,
        ProfilePhotoUrl = profile.ProfilePhotoUrl,
        HeightCm = profile.HeightCm,
        WeightKg = profile.WeightKg,
        ActivityLevel = profile.ActivityLevel,
        AverageSleepHours = profile.AverageSleepHours,
        WaterIntakeLitres = profile.WaterIntakeLitres,
        ExerciseDaysPerWeek = profile.ExerciseDaysPerWeek,
        MeditationFrequency = profile.MeditationFrequency,
        StressLevelBaseline = profile.StressLevelBaseline,
        EnergyLevelBaseline = profile.EnergyLevelBaseline,
        MoodLevelBaseline = profile.MoodLevelBaseline,
        BodyFatPercentage = profile.BodyFatPercentage,
        RestingHeartRate = profile.RestingHeartRate,
        Vo2Max = profile.Vo2Max,
        BloodPressureSystolic = profile.BloodPressureSystolic,
        BloodPressureDiastolic = profile.BloodPressureDiastolic,
        WearableProvider = profile.WearableProvider,
        WearableConnected = profile.WearableConnected,
        LastWearableSyncAt = profile.LastWearableSyncAt?.ToString("O"),
        OnboardingCompleted = profile.OnboardingCompleted,
        CreatedAt = profile.CreatedAt.ToString("O"),
        UpdatedAt = profile.UpdatedAt.ToString("O")
    };

    public static MvpDailyCheckInDto ToMvpDto(this DailyCheckIn checkIn) => new()
    {
        Id = checkIn.Id.ToString(),
        CheckInDate = checkIn.CheckInDate.ToString("yyyy-MM-dd"),
        Mood = checkIn.Mood.ToString(),
        MoodScore = checkIn.MoodScore ?? MoodScore(checkIn.Mood),
        EnergyLevel = checkIn.EnergyLevel ?? EnergyScore(checkIn.Energy),
        StressLevel = checkIn.StressLevel ?? 5,
        DidMoveToday = checkIn.DidMoveToday,
        DidMeditateToday = checkIn.DidMeditateToday,
        JournalEntry = checkIn.JournalEntry ?? checkIn.Note,
        CreatedAt = checkIn.CreatedAtUtc.ToString("O"),
        UpdatedAt = checkIn.UpdatedAtUtc.ToString("O")
    };

    private static int MoodScore(CheckInMood mood) => mood switch
    {
        CheckInMood.Great => 5,
        CheckInMood.Good => 4,
        CheckInMood.Okay => 3,
        CheckInMood.Low => 2,
        CheckInMood.Struggling => 1,
        _ => 3
    };

    private static int EnergyScore(CheckInEnergy energy) => energy switch
    {
        CheckInEnergy.High => 9,
        CheckInEnergy.Balanced => 6,
        CheckInEnergy.Low => 3,
        CheckInEnergy.Exhausted => 1,
        _ => 5
    };

    public static HabitTemplateDto ToDto(this HabitTemplate template) => new(
        template.Id.ToString(),
        template.Name,
        template.Category.ToString(),
        template.Description,
        template.IconKey,
        template.SortOrder);

    public static UserHabitDto ToDto(this UserHabit habit, bool completedToday) => new(
        habit.Id.ToString(),
        habit.HabitTemplateId?.ToString(),
        habit.Name,
        habit.Category?.ToString(),
        habit.IsCustom,
        habit.IsActive,
        completedToday);

    public static ProviderDto ToDto(this Provider provider, int serviceCount = 0, bool publicSafe = true) => new(
        provider.Id,
        provider.Name,
        provider.Slug,
        provider.ProviderType,
        publicSafe ? null : provider.LegalName,
        provider.Bio,
        provider.Lens,
        provider.Categories,
        provider.ProfileImageUrl,
        provider.CoverImageUrl,
        provider.Location,
        provider.Suburb,
        provider.City,
        provider.State,
        provider.Country,
        provider.ShowWebsitePublicly || !publicSafe ? provider.WebsiteUrl : null,
        provider.InstagramUrl,
        provider.ShowPhonePublicly || !publicSafe ? provider.Phone : null,
        provider.ShowEmailPublicly || !publicSafe ? provider.Email : null,
        provider.IsVerified,
        provider.VerificationStatus,
        provider.IsActive,
        provider.ShowEmailPublicly,
        provider.ShowPhonePublicly,
        provider.ShowWebsitePublicly,
        provider.PractitionerTitle,
        provider.YearsExperience,
        provider.Qualifications,
        provider.ServicesOffered,
        provider.Languages,
        provider.OnlineAvailable,
        provider.InPersonAvailable,
        provider.BusinessType,
        publicSafe ? null : provider.Abn,
        provider.Facilities,
        provider.AccessibilityFeatures,
        provider.TeamMembers,
        provider.OpeningHours,
        serviceCount,
        provider.CreatedUtc,
        provider.UpdatedUtc);

    public static ProviderServiceDto ToDto(this ProviderService service) => new(
        service.Id,
        service.ProviderId,
        service.Name,
        service.Description,
        service.Lens,
        service.Category,
        service.DurationMinutes,
        service.PriceAmount,
        service.PriceType,
        service.DeliveryMode,
        service.Location,
        service.IsActive,
        service.CreatedUtc,
        service.UpdatedUtc);

    public static ProviderApplicationDto ToDto(this ProviderApplication application, string? temporaryPassword = null) => new(
        application.Id,
        application.Status,
        application.ProviderType,
        application.ProviderCategory,
        application.PrimaryLens,
        application.ServiceAreas,
        application.ContactName,
        application.ContactEmail,
        application.ContactPhone,
        application.PreferredContactMethod,
        application.ProviderName,
        application.BusinessType,
        application.Abn,
        application.WebsiteUrl,
        application.InstagramUrl,
        application.OtherSocialUrl,
        application.Location,
        application.DeliveryModes,
        application.VenueNames,
        application.ServicesOffered,
        application.YearsPracticing,
        application.TypicalAudience,
        application.Bio,
        application.JoinReason,
        application.Certifications,
        application.InsuranceStatus,
        application.WorkingWithChildrenCheck,
        application.FirstAidCpr,
        application.ProfessionalMemberships,
        application.CredentialLinks,
        application.HasEventsReady,
        application.ExpectedFirstEvent,
        application.BookingTools,
        application.AdminFacingNotes,
        application.DeclarationAccepted,
        application.AdminNotes,
        application.ReviewDecisionReason,
        application.SubmittedUtc,
        application.UpdatedUtc,
        application.ReviewedUtc,
        application.ApprovedProviderId,
        temporaryPassword);

    public static Event ToEntity(this UpsertEventDto request, Guid providerId)
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            CreatedUtc = DateTimeOffset.UtcNow,
            Status = EventStatus.Published
        };
        return request.ApplyTo(evt);
    }

    public static Event ApplyTo(this UpsertEventDto request, Event evt)
    {
        evt.Title = request.Title.Trim();
        evt.Description = request.Description.Trim();
        evt.EventType = NormalizeEventType(request.EventType);
        evt.Lens = request.Lens;
        evt.ProviderServiceId = request.ProviderServiceId;
        evt.LocationName = request.LocationName.Trim();
        evt.IsOnline = request.IsOnline;
        evt.StartUtc = request.StartUtc;
        evt.EndUtc = request.EndUtc;
        evt.PriceAmount = request.PriceAmount;
        evt.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
        evt.Capacity = request.Capacity;
        evt.SignupType = request.SignupType;
        evt.ExternalBookingUrl = request.ExternalBookingUrl;
        evt.ImageUrl = request.ImageUrl;
        evt.Tags = NormalizeList(request.Tags);
        evt.BringItems = NormalizeList(request.BringItems);
        evt.BringNotes = NormalizeNullable(request.BringNotes);
        evt.Facilities = NormalizeList(request.Facilities);
        evt.AccessibilityFeatures = NormalizeList(request.AccessibilityFeatures);
        evt.PhysicalIntensity = NormalizeSingle(request.PhysicalIntensity);
        evt.SocialInteractionLevel = NormalizeSingle(request.SocialInteractionLevel);
        evt.ExperienceLevel = NormalizeSingle(request.ExperienceLevel);
        evt.AtmosphereTags = NormalizeList(request.AtmosphereTags);
        evt.FoodProvided = request.FoodProvided;
        evt.DrinksProvided = request.DrinksProvided;
        evt.DietaryOptions = NormalizeList(request.DietaryOptions);
        evt.FoodNotes = NormalizeNullable(request.FoodNotes);
        evt.AgeRestriction = NormalizeSingle(request.AgeRestriction);
        evt.SafetyNotes = NormalizeNullable(request.SafetyNotes);

        // Retreat-specific fields only persist for retreats; other event types stay clean.
        if (evt.EventType == RetreatEventType)
        {
            evt.RetreatDuration = NormalizeNullable(request.RetreatDuration);
            evt.AccommodationIncluded = request.AccommodationIncluded;
            evt.MealsIncluded = request.MealsIncluded;
            evt.TransportIncluded = request.TransportIncluded;
            evt.RetreatFocus = NormalizeSingle(request.RetreatFocus);
            evt.DifficultyLevel = NormalizeSingle(request.DifficultyLevel);
            evt.WhatsIncluded = NormalizeNullable(request.WhatsIncluded);
            evt.WhatToBring = NormalizeNullable(request.WhatToBring);
            evt.FacilitiesAvailable = NormalizeList(request.FacilitiesAvailable);
        }
        else
        {
            evt.RetreatDuration = null;
            evt.AccommodationIncluded = false;
            evt.MealsIncluded = false;
            evt.TransportIncluded = false;
            evt.RetreatFocus = null;
            evt.DifficultyLevel = null;
            evt.WhatsIncluded = null;
            evt.WhatToBring = null;
            evt.FacilitiesAvailable = [];
        }
        return evt;
    }

    private static string NormalizeEventType(string? value)
    {
        var normalized = NormalizeSingle(value);
        return normalized is not null && EventTypeOptions.Contains(normalized) ? normalized : DefaultEventType;
    }

    public static bool TryValidate(this UpsertEventDto request, out string message)
    {
        if (!IsAllowed(request.PhysicalIntensity, IntensityOptions))
        {
            message = "Physical intensity must be low, medium, or high.";
            return false;
        }

        if (!IsAllowed(request.SocialInteractionLevel, IntensityOptions))
        {
            message = "Social interaction level must be low, medium, or high.";
            return false;
        }

        if (!IsAllowed(request.ExperienceLevel, ExperienceLevelOptions))
        {
            message = "Experience level is not a supported option.";
            return false;
        }

        if (!IsAllowed(request.AgeRestriction, AgeRestrictionOptions))
        {
            message = "Age restriction is not a supported option.";
            return false;
        }

        var normalizedEventType = NormalizeSingle(request.EventType);
        if (normalizedEventType is not null && !EventTypeOptions.Contains(normalizedEventType))
        {
            message = "Event type is not a supported option.";
            return false;
        }

        if (!IsAllowed(request.RetreatFocus, RetreatFocusOptions))
        {
            message = "Retreat focus is not a supported option.";
            return false;
        }

        if (!IsAllowed(request.DifficultyLevel, DifficultyLevelOptions))
        {
            message = "Difficulty level is not a supported option.";
            return false;
        }

        if (!IsListAllowed(request.FacilitiesAvailable, FacilitiesAvailableOptions))
        {
            message = "Facilities available contains an unsupported option.";
            return false;
        }

        // Retreats require their core descriptive fields so the experience stays premium.
        if (normalizedEventType == RetreatEventType)
        {
            if (IsBlank(request.RetreatDuration)
                || NormalizeSingle(request.RetreatFocus) is null
                || NormalizeSingle(request.DifficultyLevel) is null
                || IsBlank(request.WhatsIncluded)
                || IsBlank(request.WhatToBring))
            {
                message = "Retreats need a duration, focus, difficulty level, what's included, and what to bring.";
                return false;
            }
        }

        message = "";
        return true;
    }

    private static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);

    private static bool IsListAllowed(string[]? values, HashSet<string> allowed) =>
        values is null || values.All(value => IsAllowed(value, allowed));

    public static decimal Average(DailyCheckIn[] items, Func<DailyCheckIn, int> selector) =>
        Math.Round(items.Average(item => (decimal)selector(item)), 1, MidpointRounding.AwayFromZero);

    public static IQueryable<EventDto> ProjectEvents(IQueryable<Event> query, WithinDbContext db, Guid? userId) =>
        from evt in query
            join provider in db.Providers on evt.ProviderId equals provider.Id
            let providerService = db.ProviderServices.FirstOrDefault(service => service.Id == evt.ProviderServiceId && service.IsActive)
            select new EventDto(
            evt.Id,
            evt.ProviderId,
            evt.ProviderServiceId,
            provider.Name,
            new ProviderSummaryDto(
                provider.Id,
                provider.Name,
                provider.ProviderType,
                provider.Lens,
                provider.Location,
                provider.IsVerified,
                provider.VerificationStatus),
            providerService == null
                ? null
                : new ProviderServiceDto(
                    providerService.Id,
                    providerService.ProviderId,
                    providerService.Name,
                    providerService.Description,
                    providerService.Lens,
                    providerService.Category,
                    providerService.DurationMinutes,
                    providerService.PriceAmount,
                    providerService.PriceType,
                    providerService.DeliveryMode,
                    providerService.Location,
                    providerService.IsActive,
                    providerService.CreatedUtc,
                    providerService.UpdatedUtc),
            evt.Title,
            evt.Description,
            evt.EventType == "" ? "class" : evt.EventType,
            evt.Lens,
            evt.LocationName,
            evt.IsOnline,
            evt.StartUtc,
            evt.EndUtc,
            evt.PriceAmount,
            evt.Currency,
            evt.Capacity,
            db.EventRegistrations.Count(reg => reg.EventId == evt.Id && reg.State == EventJoinState.Going),
            userId != null && db.SavedEvents.Any(saved => saved.EventId == evt.Id && saved.UserId == userId),
            userId == null ? null : db.EventRegistrations.Where(reg => reg.EventId == evt.Id && reg.UserId == userId).Select(reg => (EventJoinState?)reg.State).FirstOrDefault(),
            userId == null ? null : db.EventRegistrations.Where(reg => reg.EventId == evt.Id && reg.UserId == userId).Select(reg => (RsvpVisibility?)reg.Visibility).FirstOrDefault(),
            evt.SignupType,
            evt.ExternalBookingUrl,
            evt.ImageUrl,
            evt.Status,
            evt.Tags,
            evt.BringItems,
            evt.BringNotes,
            evt.Facilities,
            evt.AccessibilityFeatures,
            evt.PhysicalIntensity,
            evt.SocialInteractionLevel,
            evt.ExperienceLevel,
            evt.AtmosphereTags,
            evt.FoodProvided,
            evt.DrinksProvided,
            evt.DietaryOptions,
            evt.FoodNotes,
            evt.AgeRestriction,
            evt.SafetyNotes,
            evt.RetreatDuration,
            evt.AccommodationIncluded,
            evt.MealsIncluded,
            evt.TransportIncluded,
            evt.RetreatFocus,
            evt.DifficultyLevel,
            evt.WhatsIncluded,
            evt.WhatToBring,
            evt.FacilitiesAvailable);

    public static IQueryable<CommunityDto> ProjectCommunities(IQueryable<Community> query, WithinDbContext db, Guid? userId) =>
        query.Select(item => new CommunityDto(
            item.Id,
            item.ProviderId,
            item.Name,
            item.Description,
            item.Lens,
            item.Location,
            db.CommunityMembers.Count(member => member.CommunityId == item.Id),
            userId != null && db.CommunityMembers.Any(member => member.CommunityId == item.Id && member.UserId == userId)));

    public static IQueryable<PostDto> ProjectPosts(IQueryable<Post> query, WithinDbContext db) =>
        from post in query
        join user in db.Users on post.AuthorUserId equals user.Id
        orderby post.CreatedUtc descending
        select new PostDto(
            post.Id,
            post.CommunityId,
            post.EventId,
            user.DisplayName,
            post.Body,
            db.Reactions.Count(reaction => reaction.PostId == post.Id),
            db.Comments.Count(comment => comment.PostId == post.Id && !comment.IsHidden),
            post.CreatedUtc);

    public static IQueryable<CommentDto> ProjectComments(IQueryable<Comment> query, WithinDbContext db, Guid? userId = null) =>
        from comment in query
        join user in db.Users on comment.AuthorUserId equals user.Id
        orderby comment.CreatedUtc
        select new CommentDto(
            comment.Id,
            comment.ParentCommentId,
            user.DisplayName,
            comment.Body,
            comment.CreatedUtc,
            db.Reactions.Count(reaction => reaction.CommentId == comment.Id && reaction.Kind == "like"),
            userId != null && db.Reactions.Any(reaction => reaction.CommentId == comment.Id && reaction.UserId == userId && reaction.Kind == "like"));

    private static string[] NormalizeList(string[]? values) =>
        values is null
            ? []
            : values.Select(value => value.Trim().ToLowerInvariant())
                .Where(value => value.Length > 0)
                .Distinct()
                .ToArray();

    private static string? NormalizeNullable(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? NormalizeSingle(string? value)
    {
        var trimmed = value?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsAllowed(string? value, HashSet<string> allowed)
    {
        var normalized = NormalizeSingle(value);
        return normalized is null || allowed.Contains(normalized);
    }
}
