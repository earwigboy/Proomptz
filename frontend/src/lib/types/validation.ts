/**
 * Type definitions for form validation
 */

/**
 * Validation state for template usage form
 */
export interface ValidationState {
  /** Whether the form passes all validation */
  isFormValid: boolean;

  /** Whether the form can be submitted */
  canSubmit: boolean;

  /** Human-readable message explaining validation state */
  validationMessage: string | null;

  /** List of placeholder names that are missing values */
  missingPlaceholders: string[];
}

/**
 * Return type for useFormValidation hook
 */
export interface UseFormValidationResult extends ValidationState {
  /** Validate the current form state */
  validate: () => boolean;
}
