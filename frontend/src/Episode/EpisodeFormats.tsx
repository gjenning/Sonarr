import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';

interface EpisodeFormatsProps {
  formats: Array<{ id: number; name: string }>;
}

function EpisodeFormats({ formats }: EpisodeFormatsProps) {
  return (
    <div>
      {formats.map(({ id, name }) => (
        <Label key={id} kind={kinds.INFO}>
          {name}
        </Label>
      ))}
    </div>
  );
}

export default EpisodeFormats;
