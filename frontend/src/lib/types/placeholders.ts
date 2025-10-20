/**
 * Type definitions for placeholder management in template usage
 */

/**
 * Information about a single template placeholder
 */
export interface PlaceholderInfo {
  /** Placeholder identifier extracted from template (e.g., "project_name") */
  name: string;

  /** User-provided value for this placeholder */
  value: string;

  /** Optional human-readable label for the placeholder */
  displayName?: string;

  /** Whether this placeholder must have a value */
  isRequired: boolean;

  /** Validation error message, if any */
  error?: string;

  /** Whether the user has interacted with this field */
  touched: boolean;
}

/**
 * Aggregated state for all placeholders in a template
 */
export interface PlaceholderFormState {
  /** Map of placeholder name to placeholder info */
  placeholders: Record<string, PlaceholderInfo>;

  /** Whether all required placeholders have valid values */
  isValid: boolean;

  /** Map of placeholder name to error message (only includes fields with errors) */
  errors: Record<string, string>;

  /** Set of placeholder names the user has interacted with */
  touchedFields: Set<string>;
}

/**
 * Return type for usePlaceholders hook
 */
export interface UsePlaceholdersResult {
  /** Array of all placeholder info objects */
  placeholders: PlaceholderInfo[];

  /** Map of placeholder name to current value */
  placeholderValues: Record<string, string>;

  /** Whether all required placeholders are filled */
  isValid: boolean;

  /** Map of placeholder name to error message */
  errors: Record<string, string>;

  /** Update a placeholder's value */
  updatePlaceholder: (name: string, value: string) => void;

  /** Mark a placeholder as touched */
  touchPlaceholder: (name: string) => void;

  /** Get all placeholder values as a map */
  getPlaceholderValues: () => Record<string, string>;

  /** Reset all placeholder values and validation state */
  reset: () => void;
}
