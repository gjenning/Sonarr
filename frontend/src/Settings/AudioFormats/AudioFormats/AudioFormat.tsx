import React, { useCallback, useState } from 'react';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.module.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import { icons, kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { AudioFormatSpecification } from 'typings/AudioFormat';
import translate from 'Utilities/String/translate';
import EditAudioFormatModal from './EditAudioFormatModal';
import { useDeleteAudioFormat } from './useAudioFormats';

interface AudioFormatProps {
  id: number;
  name: string;
  specifications: AudioFormatSpecification[];
  onCloneAudioFormatPress: (id: number) => void;
}

function AudioFormat({
  id,
  name,
  specifications,
  onCloneAudioFormatPress,
}: AudioFormatProps) {
  const { deleteAudioFormat, isDeleting } = useDeleteAudioFormat(id);

  const [isEditAudioFormatModalOpen, setIsEditAudioFormatModalOpen] =
    useState(false);
  const [isDeleteAudioFormatModalOpen, setIsDeleteAudioFormatModalOpen] =
    useState(false);

  const onEditAudioFormatPress = useCallback(() => {
    setIsEditAudioFormatModalOpen(true);
  }, []);

  const handleEditAudioFormatModalClose = useCallback(() => {
    setIsEditAudioFormatModalOpen(false);
  }, []);

  const handleDeleteAudioFormatPress = useCallback(() => {
    setIsDeleteAudioFormatModalOpen(true);
  }, []);

  const handleDeleteAudioFormatModalClose = useCallback(() => {
    setIsDeleteAudioFormatModalOpen(false);
  }, []);

  const handleConfirmDeleteAudioFormat = useCallback(() => {
    deleteAudioFormat();
  }, [deleteAudioFormat]);

  const handleCloneAudioFormatPressHandler = useCallback(() => {
    onCloneAudioFormatPress(id);
  }, [id, onCloneAudioFormatPress]);

  const conditionCount = specifications.length;
  const isActive = conditionCount > 0;

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditCustomAudioFormatName', { name })}
      actions={
        <>
          <SettingsCardAction
            title={translate('CloneCustomAudioFormat')}
            aria-label={translate('CloneCustomAudioFormat')}
            name={icons.CLONE}
            onPress={handleCloneAudioFormatPressHandler}
          />

          <SettingsCardAction
            title={translate('DeleteCustomAudioFormat')}
            aria-label={translate('DeleteCustomAudioFormat')}
            name={icons.DELETE}
            onPress={handleDeleteAudioFormatPress}
          />
        </>
      }
      onPress={onEditAudioFormatPress}
    >
      <SettingsCardStatus
        dot={isActive ? 'active' : 'muted'}
        segments={[translate('ConditionsCount', { count: conditionCount })]}
      />

      {conditionCount ? (
        <div className={settingsCardStyles.labels}>
          {specifications.map((specification) => {
            if (!specification) {
              return null;
            }

            let kind: Kind = kinds.DEFAULT;

            if (specification.required) {
              kind = kinds.SUCCESS;
            }

            if (specification.negate) {
              kind = kinds.DANGER;
            }

            return (
              <Label
                key={specification.id}
                className={settingsCardStyles.truncatedLabel}
                kind={kind}
                dot={specification.required || specification.negate}
              >
                <MiddleTruncate text={specification.name} />
              </Label>
            );
          })}
        </div>
      ) : null}

      <EditAudioFormatModal
        id={id}
        isOpen={isEditAudioFormatModalOpen}
        onModalClose={handleEditAudioFormatModalClose}
      />

      <ConfirmModal
        isOpen={isDeleteAudioFormatModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteCustomAudioFormat')}
        message={translate('DeleteCustomAudioFormatMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDeleteAudioFormat}
        onCancel={handleDeleteAudioFormatModalClose}
      />
    </SettingsCard>
  );
}

export default AudioFormat;
