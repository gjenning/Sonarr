import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.module.css';
import translate from 'Utilities/String/translate';
import AudioFormat from './AudioFormat';
import EditAudioFormatModal from './EditAudioFormatModal';
import { useSortedAudioFormats } from './useAudioFormats';

function AudioFormats() {
  const {
    data: items,
    error,
    isFetching,
    isFetched: isPopulated,
  } = useSortedAudioFormats();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [cloneId, setCloneId] = useState<number>();

  const handleAddAudioFormatPress = useCallback(() => {
    setCloneId(undefined);
    setIsEditModalOpen(true);
  }, []);

  const handleCloneAudioFormatPress = useCallback((id: number) => {
    setCloneId(id);
    setIsEditModalOpen(true);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
    setCloneId(undefined);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('CustomAudioFormatsLoadError')}
      isFetching={isFetching}
      isPopulated={isPopulated}
      error={error}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <AudioFormat
              key={item.id}
              {...item}
              onCloneAudioFormatPress={handleCloneAudioFormatPress}
            />
          );
        })}

        <AddCard
          label={translate('AddCustomAudioFormat')}
          onPress={handleAddAudioFormatPress}
        />
      </div>

      <EditAudioFormatModal
        isOpen={isEditModalOpen}
        cloneId={cloneId}
        onModalClose={handleEditModalClose}
      />
    </PageSectionContent>
  );
}

export default AudioFormats;
