using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class add_custom_audio_formatsFixture : MigrationTest<add_custom_audio_formats>
{
    [Test]
    public void should_create_audio_formats_table()
    {
        var conn = WithDapperMigrationTestDb();

        var tables = conn.Query<string>(
            "SELECT \"name\" FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"name\" = 'AudioFormats'").ToList();

        tables.Should().BeEquivalentTo("AudioFormats");

        var columns = conn.Query<string>(
            "SELECT \"name\" FROM pragma_table_info('AudioFormats')").ToList();

        columns.Should().Contain("Name");
        columns.Should().Contain("Specifications");
    }

    [Test]
    public void should_add_not_null_audio_columns_to_quality_profiles()
    {
        var conn = WithDapperMigrationTestDb();

        var columns = conn.Query<AudioColumn237>(
            "SELECT \"name\" AS Name, \"notnull\" AS NotNullFlag FROM pragma_table_info('QualityProfiles') WHERE \"name\" LIKE 'Audio%' OR \"name\" IN ('MinAudioFormatScore', 'CutoffAudioFormatScore', 'UpgradeForAudio')").ToList();

        columns.Should().HaveCount(5);
        columns.Should().OnlyContain(c => c.NotNullFlag);
    }

    [Test]
    public void should_add_audio_columns_with_defaults_to_existing_profiles()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("QualityProfiles").Row(new
            {
                Name = "Test",
                Cutoff = 1,
                Items = "[]"
            });
        });

        var profiles = db.Query<Profile237>(
            "SELECT \"AudioFormatItems\", \"MinAudioFormatScore\", \"CutoffAudioFormatScore\", \"UpgradeForAudio\", \"AudioUpgradeRequiresTitleHint\" FROM \"QualityProfiles\" LIMIT 1");

        var profile = profiles.Single();

        profile.AudioFormatItems.Should().Be("[]");
        profile.MinAudioFormatScore.Should().Be(0);
        profile.CutoffAudioFormatScore.Should().Be(0);
        profile.UpgradeForAudio.Should().BeFalse();
        profile.AudioUpgradeRequiresTitleHint.Should().BeTrue();
    }
}

internal class Profile237
{
    public string AudioFormatItems { get; set; }
    public int MinAudioFormatScore { get; set; }
    public int CutoffAudioFormatScore { get; set; }
    public bool UpgradeForAudio { get; set; }
    public bool AudioUpgradeRequiresTitleHint { get; set; }
}

internal class AudioColumn237
{
    public string Name { get; set; }
    public bool NotNullFlag { get; set; }
}
