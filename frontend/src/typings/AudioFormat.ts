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
}

export interface AudioFormat extends ModelBase {
  name: string;
  specifications: AudioFormatSpecification[];
}
