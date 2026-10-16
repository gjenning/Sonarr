import { useCallback, useMemo } from 'react';
import { useProviderSchema } from 'Settings/useProviderSchema';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import { AudioFormat, AudioFormatSpecification } from 'typings/AudioFormat';
import { sortByProp } from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';

const PATH = '/audioformat';

const DEFAULT_AUDIO_FORMAT: AudioFormat = {
  id: 0,
  name: '',
  specifications: [],
};

export const useAudioFormats = () => {
  return useProviderSettings<AudioFormat>({
    path: PATH,
    queryOptions: {
      refetchOnWindowFocus: false,
    },
  });
};

export const useSortedAudioFormats = () => {
  const result = useAudioFormats();

  const sortedData = useMemo(
    () => [...result.data].sort(sortByProp('name')),
    [result.data]
  );

  return { ...result, data: sortedData };
};

export const useAudioFormat = (id: number | undefined) => {
  const { data } = useAudioFormats();

  if (id === undefined) {
    return undefined;
  }

  return data.find((af) => af.id === id);
};

export const useAudioFormatsWithIds = (ids: number[]) => {
  const { data } = useAudioFormats();

  return data.filter((af) => ids.includes(af.id));
};

export const useDeleteAudioFormat = (id: number) => {
  const result = useDeleteProvider<AudioFormat>(id, PATH);

  return {
    ...result,
    deleteAudioFormat: result.deleteProvider,
  };
};

export const useAudioFormatSchema = (enabled: boolean = true) => {
  return useProviderSchema<AudioFormatSpecification>(PATH, enabled);
};

function getNextSpecId(specifications: AudioFormatSpecification[]) {
  return specifications.length > 0
    ? Math.max(...specifications.map((s) => s.id)) + 1
    : 1;
}

export const useManageAudioFormat = (
  id: number | undefined,
  cloneId: number | undefined
) => {
  const cloneSource = useAudioFormat(cloneId);

  if (cloneId && !cloneSource) {
    throw new Error(`AudioFormat with ID ${cloneId} not found`);
  }

  const defaultProvider = useMemo<AudioFormat>(() => {
    if (cloneId && cloneSource) {
      return {
        ...cloneSource,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneSource.name,
        }),
      };
    }

    return DEFAULT_AUDIO_FORMAT;
  }, [cloneId, cloneSource]);

  const manage = useManageProviderSettings<AudioFormat>(
    id,
    defaultProvider,
    PATH
  );

  const specifications = useMemo(
    () =>
      manage.item.specifications.value.map((spec, i) => ({
        ...spec,
        id: spec.id ?? i + 1,
      })),
    [manage.item.specifications.value]
  );

  const saveSpecification = useCallback(
    (spec: AudioFormatSpecification) => {
      if (spec.id > 0 && specifications.some((s) => s.id === spec.id)) {
        manage.updateValue(
          'specifications',
          specifications.map((s) => (s.id === spec.id ? spec : s))
        );
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        { ...spec, id: newId },
      ]);
    },
    [specifications, manage]
  );

  const deleteSpecification = useCallback(
    (specId: number) => {
      manage.updateValue(
        'specifications',
        specifications.filter((s) => s.id !== specId)
      );
    },
    [specifications, manage]
  );

  const cloneSpecification = useCallback(
    (specId: number) => {
      const spec = specifications.find((s) => s.id === specId);

      if (!spec) {
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        {
          ...spec,
          id: newId,
          name: translate('DefaultNameCopiedSpecification', {
            name: spec.name,
          }),
        },
      ]);
    },
    [specifications, manage]
  );

  return {
    ...manage,
    saveAudioFormat: manage.saveProvider,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  };
};
