// The /audioformat/schema endpoint exposes a single AudioStreamSpecification whose `field`
// select options come from the AudioStreamField enum and whose `value` is one free-text
// field; the schema carries no per-field widget hints, so the widget map is hardcoded here
// keyed by the enum member name (see AudioStreamSpecification.cs).
export type AudioStreamValueWidget = 'text' | 'number' | 'language' | 'boolean';

const widgetByFieldName: Record<string, AudioStreamValueWidget> = {
  Codec: 'text',
  Channels: 'text',
  Title: 'text',
  Bitrate: 'number',
  StreamCount: 'number',
  Language: 'language',
  AudioDescription: 'boolean',
};

export function getValueWidget(
  fieldName: string | undefined
): AudioStreamValueWidget {
  return (fieldName && widgetByFieldName[fieldName]) || 'text';
}

// Numeric fields (Bitrate, StreamCount) store an optional comparison operator prefix
// (>=, <=, >, <, =) followed by a number; a bare number means >=. The numeric input only
// edits the number, so the operator is parsed out for display and re-applied on save to
// preserve values created with an operator through the API.
const comparisonRegex = /^(>=|<=|>|<|=)?\s*(\d+(?:\.\d+)?)$/;

export function parseComparison(value: string): {
  operator: string;
  number: number | null;
} {
  const match = comparisonRegex.exec(value.trim());

  if (!match) {
    return { operator: '>=', number: null };
  }

  return { operator: match[1] ?? '>=', number: parseFloat(match[2]) };
}

export function serializeComparison(
  operator: string,
  number: number | null
): string {
  if (number == null || Number.isNaN(number)) {
    return '';
  }

  return `${operator}${number}`;
}

// The Language field stores comma-separated language ids (e.g. "1,3").
export function parseLanguageIds(value: string): number[] {
  return value
    .split(',')
    .map((part) => part.trim())
    .filter((part) => part !== '' && /^\d+$/.test(part))
    .map((part) => parseInt(part));
}

export function serializeLanguageIds(ids: number[]): string {
  return ids.join(',');
}
