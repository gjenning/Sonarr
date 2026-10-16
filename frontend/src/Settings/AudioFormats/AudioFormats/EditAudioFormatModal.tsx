import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditAudioFormatModalContent, {
  EditAudioFormatModalContentProps,
} from './EditAudioFormatModalContent';

interface EditAudioFormatModalProps extends EditAudioFormatModalContentProps {
  isOpen: boolean;
}

function EditAudioFormatModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditAudioFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <EditAudioFormatModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditAudioFormatModal;
