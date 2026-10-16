import classNames from 'classnames';
import React, { useCallback, useEffect, useState } from 'react';
import Card from 'Components/Card';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes } from 'Helpers/Props';
import { AudioFormat, AudioFormatSpecification } from 'typings/AudioFormat';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import EditSpecificationModal from './Specifications/EditSpecificationModal';
import Specification from './Specifications/Specification';
import { useAudioFormatSchema, useManageAudioFormat } from './useAudioFormats';
import styles from './EditAudioFormatModalContent.module.css';

export interface EditAudioFormatModalContentProps {
  id?: number;
  cloneId?: number;
  onModalClose: () => void;
}

type AudioFormatSchemaItem = AudioFormatSpecification & {
  presets?: AudioFormatSpecification[];
};

function EditAudioFormatModalContent({
  id,
  cloneId,
  onModalClose,
}: EditAudioFormatModalContentProps) {
  const {
    item,
    validationErrors,
    validationWarnings,
    updateValue,
    saveAudioFormat,
    isSaving,
    saveError,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  } = useManageAudioFormat(id, cloneId);

  const { schema } = useAudioFormatSchema();

  const [isEditSpecificationModalOpen, setIsEditSpecificationModalOpen] =
    useState(false);
  const [editingSpecification, setEditingSpecification] =
    useState<AudioFormatSpecification | null>(null);

  const { name } = item;
  const wasSaving = usePrevious(isSaving);

  // There is a single specification implementation (AudioStreamSpecification), so adding
  // a condition skips the implementation picker used by custom formats and seeds the new
  // specification directly from the /audioformat/schema template.
  const handleAddSpecificationPress = useCallback(() => {
    const template = schema[0] as AudioFormatSchemaItem | undefined;

    if (!template) {
      return;
    }

    const { presets: _unused, ...specTemplate } = template;

    setEditingSpecification({ ...specTemplate, id: 0, name: '' });
    setIsEditSpecificationModalOpen(true);
  }, [schema]);

  const handleEditSpecificationModalClose = useCallback(() => {
    setIsEditSpecificationModalOpen(false);
    setEditingSpecification(null);
  }, []);

  const handleSaveNewSpecification = useCallback(
    (spec: AudioFormatSpecification) => {
      saveSpecification(spec);
    },
    [saveSpecification]
  );

  const handleInputChange = useCallback(
    ({ name: key, value }: InputChanged) => {
      updateValue(
        key as keyof AudioFormat,
        value as AudioFormat[keyof AudioFormat]
      );
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveAudioFormat();
  }, [saveAudioFormat]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
        {id
          ? translate('EditCustomAudioFormat')
          : translate('AddCustomAudioFormat')}
      </ModalHeader>
      <ModalBody>
        <div>
          <Form
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FormRow>
              <FormLabel>{translate('Name')}</FormLabel>

              <FormInput
                type={inputTypes.TEXT}
                name="name"
                {...name}
                onChange={handleInputChange}
              />
            </FormRow>
          </Form>

          <div className={styles.conditions}>
            <h3 className={styles.conditionsTitle}>
              {translate('Conditions')}
            </h3>

            <p className={styles.intro}>
              {translate('CustomAudioFormatsMatchInfo')}
            </p>

            <div className={styles.specifications}>
              {specifications.map((specification) => (
                <Specification
                  key={specification.id}
                  {...specification}
                  onSaveSpecification={saveSpecification}
                  onCloneSpecificationPress={cloneSpecification}
                  onConfirmDeleteSpecification={deleteSpecification}
                />
              ))}

              <Card
                className={classNames(
                  styles.addSpecification,
                  !specifications.length && styles.fullWidth
                )}
                onPress={handleAddSpecificationPress}
              >
                <div className={styles.center}>
                  <Icon name={icons.ADD} size={20} />
                </div>
                <div className={styles.addLabel}>
                  {translate('AddCondition')}
                </div>
              </Card>
            </div>
          </div>

          {editingSpecification ? (
            <EditSpecificationModal
              isOpen={isEditSpecificationModalOpen}
              specification={editingSpecification}
              onSave={handleSaveNewSpecification}
              onModalClose={handleEditSpecificationModalClose}
            />
          ) : null}
        </div>
      </ModalBody>
      <ModalFooter>
        <div className={styles.rightButtons} />

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditAudioFormatModalContent;
