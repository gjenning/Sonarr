using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(237)]
    public class add_custom_audio_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Add Audio Format Table
            Create.TableForModel("AudioFormats")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Specifications").AsString().WithDefaultValue("[]");

            // Add Audio Format Columns to Quality Profiles
            Alter.Table("QualityProfiles").AddColumn("AudioFormatItems").AsString().WithDefaultValue("[]");
            Alter.Table("QualityProfiles").AddColumn("MinAudioFormatScore").AsInt32().WithDefaultValue(0);
            Alter.Table("QualityProfiles").AddColumn("CutoffAudioFormatScore").AsInt32().WithDefaultValue(0);
            Alter.Table("QualityProfiles").AddColumn("UpgradeForAudio").AsBoolean().WithDefaultValue(false);
            Alter.Table("QualityProfiles").AddColumn("AudioUpgradeRequiresTitleHint").AsBoolean().WithDefaultValue(true);
        }
    }
}
