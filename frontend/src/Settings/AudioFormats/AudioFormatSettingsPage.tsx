import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import settingsStyles from 'Settings/Settings.module.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import AudioFormats from './AudioFormats/AudioFormats';

function AudioFormatSettingsPage() {
  return (
    <SettingsPage
      title={translate('CustomAudioFormatsSettings')}
      showSave={false}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('CustomAudioFormats')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('CustomAudioFormats')}
              description={translate('CustomAudioFormatsSectionDescription')}
            />

            <AudioFormats />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default AudioFormatSettingsPage;
