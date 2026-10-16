import React, { useMemo } from 'react';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import { QualityProfileAudioFormatItem as QualityProfileAudioFormatItemModel } from 'Settings/Profiles/Quality/useQualityProfiles';
import { Failure } from 'typings/pending';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItem from './QualityProfileFormatItem';
import styles from './QualityProfileFormatItems.module.css';

interface QualityProfileAudioFormatItemsProps {
  profileAudioFormatItems: QualityProfileAudioFormatItemModel[];
  errors?: Failure[];
  warnings?: Failure[];
  onQualityProfileAudioFormatItemScoreChange: (
    formatId: number,
    score: number
  ) => void;
}

function QualityProfileAudioFormatItems({
  profileAudioFormatItems,
  errors = [],
  warnings = [],
  onQualityProfileAudioFormatItemScoreChange,
}: QualityProfileAudioFormatItemsProps) {
  const order = useMemo(() => {
    const items = profileAudioFormatItems.reduce<Record<number, number>>(
      (acc, cur, index) => {
        acc[cur.format] = index;
        return acc;
      },
      {}
    );

    return [...profileAudioFormatItems]
      .sort((a, b) => {
        if (b.score !== a.score) {
          return b.score - a.score;
        }

        return a.name.localeCompare(b.name, undefined, { numeric: true });
      })
      .map((x) => items[x.format]);
  }, [profileAudioFormatItems]);

  if (profileAudioFormatItems.length < 1) {
    return (
      <InlineMarkdown
        className={styles.addCustomFormatMessage}
        data={translate('WantMoreControlAddACustomAudioFormat')}
      />
    );
  }

  return (
    <div>
      <FormInputHelpText text={translate('CustomAudioFormatHelpText')} />

      {errors.map((error, index) => {
        return (
          <FormInputHelpText
            key={index}
            text={error.message}
            isError={true}
            isCheckInput={false}
          />
        );
      })}

      {warnings.map((warning, index) => {
        return (
          <FormInputHelpText
            key={index}
            text={warning.message}
            isWarning={true}
            isCheckInput={false}
          />
        );
      })}

      <div className={styles.formats}>
        <div className={styles.headerContainer}>
          <div className={styles.headerTitle}>
            {translate('CustomAudioFormat')}
          </div>
          <div className={styles.headerScore}>{translate('Score')}</div>
        </div>

        {order.map((index) => {
          const { format, name, score } = profileAudioFormatItems[index];
          return (
            <QualityProfileFormatItem
              key={format}
              formatId={format}
              name={name}
              score={score}
              onScoreChange={onQualityProfileAudioFormatItemScoreChange}
            />
          );
        })}
      </div>
    </div>
  );
}

export default QualityProfileAudioFormatItems;
