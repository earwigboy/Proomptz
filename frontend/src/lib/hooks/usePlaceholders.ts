import { useState, useEffect } from 'react';
import { extractPlaceholders } from '../utils/placeholders';

export function usePlaceholders(templateContent: string) {
  const [placeholders, setPlaceholders] = useState<string[]>([]);
  const [values, setValues] = useState<Record<string, string>>({});

  useEffect(() => {
    const extracted = extractPlaceholders(templateContent);
    setPlaceholders(extracted);

    // Initialize values with empty strings
    const initialValues: Record<string, string> = {};
    extracted.forEach((placeholder) => {
      initialValues[placeholder] = '';
    });
    setValues(initialValues);
  }, [templateContent]);

  const updateValue = (placeholder: string, value: string) => {
    setValues((prev) => ({ ...prev, [placeholder]: value }));
  };

  const allFilled = placeholders.every((p) => values[p] && values[p].trim() !== '');

  return {
    placeholders,
    values,
    updateValue,
    allFilled,
  };
}
