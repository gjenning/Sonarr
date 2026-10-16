import React, { useCallback, useMemo } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import { usePendingFieldsStore } from 'Helpers/Hooks/usePendingFieldsStore';
import { inputTypes, kinds } from 'Helpers/Props';
import { useFilteredLanguages } from 'Language/useLanguages';
import { AudioFormatSpecification } from 'typings/AudioFormat';
import { FieldSelectOption } from 'typings/Field';
import { InputChanged } from 'typings/inputs';
import { PendingField } from 'typings/pending';
import selectSettings from 'Utilities/selectSettings';
import translate from 'Utilities/String/translate';
import {
  AudioStreamValueWidget,
  getValueWidget,
  parseComparison,
  parseLanguageIds,
  serializeComparison,
  serializeLanguageIds,
} from './audioStreamValue';
import styles from './EditSpecificationModalContent.module.css';

export interface EditSpecificationModalContentProps {
  specification: AudioFormatSpecification;
  onSave: (spec: AudioFormatSpecification) => void;
  onDeleteSpecificationPress?: () => void;
  onModalClose: () => void;
}

// The specification's `fields` come from the /audioformat/schema endpoint, so they carry
// the full field schema (select options, help text) in addition to the current value.
type SpecificationSchemaField = PendingField<AudioFormatSpecification> & {
  helpText?: string;
  selectOptions?: FieldSelectOption<number>[];
};

function getValueHelpText(widget: AudioStreamValueWidget): string | undefined {
  switch (widget) {
    case 'number':
      return translate('AudioFormatValueHelpTextNumber');
    case 'language':
      return translate('AudioFormatValueHelpTextLanguage');
    case 'boolean':
      return translate('AudioFormatValueHelpTextAudioDescription');
    default:
      return undefined;
  }
}

function EditSpecificationModalContent({
  specification,
  onSave,
  onDeleteSpecificationPress,
  onModalClose,
}: EditSpecificationModalContentProps) {
  const { pendingChanges, setPendingChange, clearPendingChanges } =
    usePendingChangesStore<AudioFormatSpecification>({});

  const {
    pendingFields,
    setPendingField,
    hasPendingFields,
    clearPendingFields,
  } = usePendingFieldsStore();

  const {
    settings: item,
    validationErrors,
    validationWarnings,
  } = useMemo(() => {
    const combinedPendingChanges = hasPendingFields
      ? {
          ...pendingChanges,
          fields: Object.fromEntries(pendingFields),
        }
      : pendingChanges;

    return selectSettings(specification, combinedPendingChanges);
  }, [specification, pendingChanges, pendingFields, hasPendingFields]);

  const schemaFields = useMemo(
    () => (item.fields ?? []) as SpecificationSchemaField[],
    [item.fields]
  );

  const fieldField = schemaFields.find((field) => field.name === 'field');
  const valueField = schemaFields.find((field) => field.name === 'value');

  const fieldOptions = useMemo(
    () => fieldField?.selectOptions ?? [],
    [fieldField]
  );

  const fieldId = typeof fieldField?.value === 'number' ? fieldField.value : 0;
  const fieldName = fieldOptions.find(
    (option) => option.value === fieldId
  )?.name;
  const widget = getValueWidget(fieldName);

  const valueString = valueField?.value == null ? '' : String(valueField.value);

  const { name, negate, required } = item;
  const { implementationName } = item;

  const { data: languages = [] } = useFilteredLanguages({
    Any: true,
    Original: true,
  });

  const fieldSelectValues = useMemo(
    () =>
      fieldOptions.map((option) => ({ key: option.value, value: option.name })),
    [fieldOptions]
  );

  const languageSelectValues = useMemo(
    () =>
      languages.map((language) => ({ key: language.id, value: language.name })),
    [languages]
  );

  const booleanSelectValues = useMemo(
    () => [
      { key: 'true', value: translate('Yes') },
      { key: 'false', value: translate('No') },
    ],
    []
  );

  const comparison = useMemo(() => parseComparison(valueString), [valueString]);

  const valueHelpText = getValueHelpText(widget);

  const handleInputChange = useCallback(
    ({ name: key, value }: InputChanged) => {
      setPendingChange(
        key as keyof AudioFormatSpecification,
        value as AudioFormatSpecification[keyof AudioFormatSpecification]
      );
    },
    [setPendingChange]
  );

  const handleFieldChange = useCallback(
    ({ name: fieldName_, value }: InputChanged) => {
      setPendingField(fieldName_, value);
    },
    [setPendingField]
  );

  const handleFieldSelectChange = useCallback(
    ({ value }: InputChanged) => {
      // The value is interpreted per field (regex, number, language ids, boolean), so
      // switching field resets it instead of carrying over an invalid value.
      setPendingField('field', value);
      setPendingField('value', '');
    },
    [setPendingField]
  );

  const handleNumberChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      setPendingField('value', serializeComparison(comparison.operator, value));
    },
    [comparison.operator, setPendingField]
  );

  const handleLanguageChange = useCallback(
    ({ value }: InputChanged<number[]>) => {
      setPendingField('value', serializeLanguageIds(value));
    },
    [setPendingField]
  );

  const handleCancelPress = useCallback(() => {
    clearPendingChanges();
    clearPendingFields();
    onModalClose();
  }, [clearPendingChanges, clearPendingFields, onModalClose]);

  const handleSavePress = useCallback(() => {
    let updatedSpec: AudioFormatSpecification = {
      ...specification,
      ...pendingChanges,
    };

    if (hasPendingFields) {
      updatedSpec = {
        ...updatedSpec,
        fields: specification.fields.map((field) =>
          pendingFields.has(field.name)
            ? {
                ...field,
                value: pendingFields.get(field.name) as typeof field.value,
              }
            : field
        ),
      };
    }

    onSave(updatedSpec);
    onModalClose();
  }, [
    specification,
    pendingChanges,
    pendingFields,
    hasPendingFields,
    onSave,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={handleCancelPress}>
      <ModalHeader subtitle={specification.id ? specification.name : undefined}>
        {specification.id
          ? translate('EditConditionImplementation', { implementationName })
          : translate('AddConditionImplementation', { implementationName })}
      </ModalHeader>
      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          {widget === 'text' ? (
            <p className={styles.intro}>
              <InlineMarkdown
                data={translate('ConditionUsingRegularExpressions')}
              />{' '}
              <InlineMarkdown
                data={translate('RegularExpressionsTutorialLink', {
                  url: 'https://www.regular-expressions.info/tutorial.html',
                })}
              />{' '}
              <InlineMarkdown
                data={translate('RegularExpressionsCanBeTested', {
                  url: 'http://regexstorm.net/tester',
                })}
              />
            </p>
          ) : null}

          <FormRow>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInput
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{fieldField?.label || translate('Field')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="field"
              value={fieldId}
              values={fieldSelectValues}
              errors={fieldField?.errors}
              warnings={fieldField?.warnings}
              pending={fieldField?.pending}
              onChange={handleFieldSelectChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{valueField?.label || translate('Value')}</FormLabel>

            {valueHelpText ? <FormInputHelpText text={valueHelpText} /> : null}

            {widget === 'text' ? (
              <FormInput
                type={inputTypes.TEXT}
                name="value"
                value={valueString}
                errors={valueField?.errors}
                warnings={valueField?.warnings}
                pending={valueField?.pending}
                onChange={handleFieldChange}
              />
            ) : null}

            {widget === 'number' ? (
              <FormInput
                type={inputTypes.NUMBER}
                name="value"
                value={comparison.number}
                min={0}
                unit={fieldName === 'Bitrate' ? 'kbps' : undefined}
                errors={valueField?.errors}
                warnings={valueField?.warnings}
                pending={valueField?.pending}
                onChange={handleNumberChange}
              />
            ) : null}

            {widget === 'language' ? (
              <FormInput
                type={inputTypes.SELECT}
                name="value"
                value={parseLanguageIds(valueString)}
                values={languageSelectValues}
                errors={valueField?.errors}
                warnings={valueField?.warnings}
                pending={valueField?.pending}
                onChange={handleLanguageChange}
              />
            ) : null}

            {widget === 'boolean' ? (
              <FormInput
                type={inputTypes.SELECT}
                name="value"
                value={valueString}
                values={booleanSelectValues}
                errors={valueField?.errors}
                warnings={valueField?.warnings}
                pending={valueField?.pending}
                onChange={handleFieldChange}
              />
            ) : null}
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Negate')}</FormLabel>
            <FormInputHelpText
              text={translate('NegateHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Required')}</FormLabel>
            <FormInputHelpText
              text={translate('RequiredHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="required"
              {...required}
              onChange={handleInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>
      <ModalFooter>
        {specification.id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteSpecificationPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={handleCancelPress}>{translate('Cancel')}</Button>

        <SpinnerErrorButton isSpinning={false} onPress={handleSavePress}>
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditSpecificationModalContent;
