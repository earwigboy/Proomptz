import { useState, useEffect, useMemo, useCallback } from 'react';
import { extractPlaceholders } from '../utils/placeholders';
import type { PlaceholderInfo, UsePlaceholdersResult } from '../types/placeholders';

export function usePlaceholders(templateContent: string): UsePlaceholdersResult {
  const [placeholderMap, setPlaceholderMap] = useState<Record<string, PlaceholderInfo>>({});

  useEffect(() => {
    const extracted = extractPlaceholders(templateContent);

    // Initialize placeholder info objects
    const initialMap: Record<string, PlaceholderInfo> = {};
    extracted.forEach((name) => {
      initialMap[name] = {
        name,
        value: '',
        displayName: name.replace(/_/g, ' '),
        isRequired: true,
        error: undefined,
        touched: false,
      };
    });
    setPlaceholderMap(initialMap);
  }, [templateContent]);

  const updatePlaceholder = useCallback((name: string, value: string) => {
    setPlaceholderMap((prev) => {
      const placeholder = prev[name];
      if (!placeholder) return prev;

      const error =
        placeholder.isRequired && value.trim() === ''
          ? 'This field is required'
          : undefined;

      return {
        ...prev,
        [name]: {
          ...placeholder,
          value,
          error,
        },
      };
    });
  }, []);

  const touchPlaceholder = useCallback((name: string) => {
    setPlaceholderMap((prev) => {
      const placeholder = prev[name];
      if (!placeholder || placeholder.touched) return prev;

      return {
        ...prev,
        [name]: {
          ...placeholder,
          touched: true,
        },
      };
    });
  }, []);

  const placeholders = useMemo(
    () => Object.values(placeholderMap),
    [placeholderMap]
  );

  const placeholderValues = useMemo(
    () =>
      Object.entries(placeholderMap).reduce(
        (acc, [name, info]) => {
          acc[name] = info.value;
          return acc;
        },
        {} as Record<string, string>
      ),
    [placeholderMap]
  );

  const isValid = useMemo(
    () =>
      Object.values(placeholderMap).every(
        (p) => !p.isRequired || p.value.trim() !== ''
      ),
    [placeholderMap]
  );

  const errors = useMemo(
    () =>
      Object.entries(placeholderMap).reduce(
        (acc, [name, info]) => {
          if (info.error) {
            acc[name] = info.error;
          }
          return acc;
        },
        {} as Record<string, string>
      ),
    [placeholderMap]
  );

  const getPlaceholderValues = useCallback(
    () => placeholderValues,
    [placeholderValues]
  );

  const reset = useCallback(() => {
    setPlaceholderMap((prev) => {
      const resetMap: Record<string, PlaceholderInfo> = {};
      Object.keys(prev).forEach((name) => {
        resetMap[name] = {
          name,
          value: '',
          displayName: name.replace(/_/g, ' '),
          isRequired: true,
          error: undefined,
          touched: false,
        };
      });
      return resetMap;
    });
  }, []);

  return {
    placeholders,
    placeholderValues,
    isValid,
    errors,
    updatePlaceholder,
    touchPlaceholder,
    getPlaceholderValues,
    reset,
  };
}
