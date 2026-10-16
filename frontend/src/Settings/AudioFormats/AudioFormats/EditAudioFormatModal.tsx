import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

// Placeholder for the edit modal implemented in task 4.3 (edit modal +
// specification editor). Keeps the settings page functional standalone.
interface EditAudioFormatModalProps {
  id?: number;
  cloneId?: number;
  isOpen: boolean;
  onModalClose: () => void;
}

function EditAudioFormatModal({
  id,
  isOpen,
  onModalClose,
}: EditAudioFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {id
            ? translate('EditCustomAudioFormat')
            : translate('AddCustomAudioFormat')}
        </ModalHeader>

        <ModalBody>{translate('CustomAudioFormatEditorPlaceholder')}</ModalBody>

        <ModalFooter>
          <Button kind={kinds.DEFAULT} onPress={onModalClose}>
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default EditAudioFormatModal;
