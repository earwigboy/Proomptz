import { useMemo, useCallback } from 'react';
import type { PlaceholderInfo } from '../types/placeholders';
import type { UseFormValidationResult } from '../types/validation';

/**
 * Hook for validating template usage form state
 *
 * @param placeholders - Array of placeholder info objects from usePlaceholders
 * @returns Validation state and validation function
 */
export function useFormValidation(
  placeholders: PlaceholderInfo[]
): UseFormValidationResult {
  // Check if all required placeholders have non-empty values
  const isFormValid = useMemo(
    () => placeholders.every((p) => !p.isRequired || p.value.trim() !== ''),
    [placeholders]
  );

  // Button can be clicked only if form is valid
  const canSubmit = isFormValid;

  // List of placeholder names that are missing values
  const missingPlaceholders = useMemo(
    () =>
      placeholders
        .filter((p) => p.isRequired && p.value.trim() === '')
        .map((p) => p.name),
    [placeholders]
  );

  // Generate validation message for display
  const validationMessage = useMemo(() => {
    if (canSubmit) return null;

    const count = missingPlaceholders.length;
    if (count === 0) return null;

    const names = missingPlaceholders.map((name) => name.replace(/_/g, ' '));
    return `${count} placeholder${count > 1 ? 's' : ''} required: ${names.join(', ')}`;
  }, [canSubmit, missingPlaceholders]);

  // Explicit validation function
  const validate = useCallback(() => {
    return isFormValid;
  }, [isFormValid]);

  return {
    isFormValid,
    canSubmit,
    validationMessage,
    missingPlaceholders,
    validate,
  };
}
