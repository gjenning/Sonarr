using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Profiles.Quality;

public class QualityProfileResource : RestResource
{
    public string? Name { get; set; }
    public bool UpgradeAllowed { get; set; }
    public int Cutoff { get; set; }
    public List<QualityProfileQualityItemResource> Items { get; set; } = [];
    public int MinFormatScore { get; set; }
    public int CutoffFormatScore { get; set; }
    public int MinUpgradeFormatScore { get; set; }
    public List<ProfileFormatItemResource> FormatItems { get; set; } = [];
    public List<ProfileAudioFormatItemResource> AudioFormatItems { get; set; } = [];
    public int MinAudioFormatScore { get; set; }
    public int CutoffAudioFormatScore { get; set; }
    public bool UpgradeForAudio { get; set; }
    public bool AudioUpgradeRequiresTitleHint { get; set; }
}

public class QualityProfileQualityItemResource : RestResource
{
    public string? Name { get; set; }
    public NzbDrone.Core.Qualities.Quality? Quality { get; set; }
    public List<QualityProfileQualityItemResource> Items { get; set; } = [];
    public bool Allowed { get; set; }
    public double? MinSize { get; set; }
    public double? MaxSize { get; set; }
    public double? PreferredSize { get; set; }
}

public class ProfileFormatItemResource : RestResource
{
    public int Format { get; set; }
    public string? Name { get; set; }
    public int Score { get; set; }
}

public class ProfileAudioFormatItemResource : RestResource
{
    public int Format { get; set; }
    public string? Name { get; set; }
    public int Score { get; set; }
}

public static class ProfileResourceMapper
{
    public static QualityProfileResource ToResource(this QualityProfile model)
    {
        return new QualityProfileResource
        {
            Id = model.Id,
            Name = model.Name,
            UpgradeAllowed = model.UpgradeAllowed,
            Cutoff = model.Cutoff,
            Items = model.Items.ConvertAll(ToResource),
            MinFormatScore = model.MinFormatScore,
            CutoffFormatScore = model.CutoffFormatScore,
            MinUpgradeFormatScore = model.MinUpgradeFormatScore,
            FormatItems = model.FormatItems.ConvertAll(ToResource),
            AudioFormatItems = model.AudioFormatItems.ConvertAll(ToResource),
            MinAudioFormatScore = model.MinAudioFormatScore,
            CutoffAudioFormatScore = model.CutoffAudioFormatScore,
            UpgradeForAudio = model.UpgradeForAudio,
            AudioUpgradeRequiresTitleHint = model.AudioUpgradeRequiresTitleHint
        };
    }

    public static QualityProfileQualityItemResource ToResource(this QualityProfileQualityItem model)
    {
        return new QualityProfileQualityItemResource
        {
            Id = model.Id,
            Name = model.Name,
            Quality = model.Quality,
            Items = model.Items.ConvertAll(ToResource),
            Allowed = model.Allowed,
            MinSize = model.MinSize,
            MaxSize = model.MaxSize,
            PreferredSize = model.PreferredSize
        };
    }

    public static ProfileFormatItemResource ToResource(this ProfileFormatItem model)
    {
        return new ProfileFormatItemResource
        {
            Format = model.Format.Id,
            Name = model.Format.Name,
            Score = model.Score
        };
    }

    public static ProfileAudioFormatItemResource ToResource(this ProfileAudioFormatItem model)
    {
        return new ProfileAudioFormatItemResource
        {
            Format = model.Format.Id,
            Name = model.Format.Name,
            Score = model.Score
        };
    }

    public static QualityProfile ToModel(this QualityProfileResource resource)
    {
        return new QualityProfile
        {
            Id = resource.Id,
            Name = resource.Name,
            UpgradeAllowed = resource.UpgradeAllowed,
            Cutoff = resource.Cutoff,
            Items = resource.Items.ConvertAll(ToModel),
            MinFormatScore = resource.MinFormatScore,
            CutoffFormatScore = resource.CutoffFormatScore,
            MinUpgradeFormatScore = resource.MinUpgradeFormatScore,
            FormatItems = resource.FormatItems.ConvertAll(ToModel),
            AudioFormatItems = resource.AudioFormatItems.ConvertAll(ToModel),
            MinAudioFormatScore = resource.MinAudioFormatScore,
            CutoffAudioFormatScore = resource.CutoffAudioFormatScore,
            UpgradeForAudio = resource.UpgradeForAudio,
            AudioUpgradeRequiresTitleHint = resource.AudioUpgradeRequiresTitleHint
        };
    }

    public static QualityProfileQualityItem ToModel(this QualityProfileQualityItemResource resource)
    {
        return new QualityProfileQualityItem
        {
            Id = resource.Id,
            Name = resource.Name,
            Quality = resource.Quality != null ? (NzbDrone.Core.Qualities.Quality)resource.Quality.Id : null,
            Items = resource.Items.ConvertAll(ToModel),
            Allowed = resource.Allowed,
            MinSize = resource.MinSize,
            MaxSize = resource.MaxSize,
            PreferredSize = resource.PreferredSize
        };
    }

    public static ProfileFormatItem ToModel(this ProfileFormatItemResource resource)
    {
        return new ProfileFormatItem
        {
            Format = new CustomFormat { Id = resource.Format },
            Score = resource.Score
        };
    }

    public static ProfileAudioFormatItem ToModel(this ProfileAudioFormatItemResource resource)
    {
        return new ProfileAudioFormatItem
        {
            Format = new AudioFormat { Id = resource.Format },
            Score = resource.Score
        };
    }

    public static List<QualityProfileResource> ToResource(this IEnumerable<QualityProfile> models)
    {
        return models.Select(ToResource).ToList();
    }
}
