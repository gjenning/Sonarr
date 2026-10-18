import ModelBase from 'App/ModelBase';
import Field from './Field';

export interface AudioFormatSpecification {
  id: number;
  name: string;
  implementation: string;
  implementationName: string;
  infoLink: string;
  negate: boolean;
  required: boolean;
  fields: Field[];
  // Per-field dropdown options (field name -> allowed exact values) from the schema;
  // values outside the list are still valid and matched as regex.
  valueOptions?: Record<string, string[]>;
}

export interface AudioFormat extends ModelBase {
  name: string;
  specifications: AudioFormatSpecification[];
}
