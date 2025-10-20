import type { PlaceholderInfo } from '../../lib/types/placeholders';
import { Input } from '../ui/input';
import { Textarea } from '../ui/textarea';
import { Label } from '../ui/label';

interface PlaceholderFormProps {
  /** Array of placeholders with current values and validation state */
  placeholders: PlaceholderInfo[];

  /** Callback when placeholder value changes */
  onPlaceholderChange: (name: string, value: string) => void;

  /** Callback when placeholder field is blurred (for touched state) */
  onPlaceholderBlur?: (name: string) => void;

  /** Whether form is currently disabled (e.g., during submission) */
  disabled?: boolean;

  /** Optional CSS class for container */
  className?: string;
}

export default function PlaceholderForm({
  placeholders,
  onPlaceholderChange,
  onPlaceholderBlur,
  disabled = false,
  className = '',
}: PlaceholderFormProps) {
  // Handle empty placeholder case
  if (placeholders.length === 0) {
    return (
      <div className={`space-y-4 ${className}`}>
        <p className="text-muted-foreground text-center py-8">
          This template has no placeholders to fill.
        </p>
      </div>
    );
  }

  /**
   * Determine if a field should use textarea based on characteristics
   * - Use textarea if name contains "description", "content", "text", "message"
   * - Use textarea if current value length > 50 characters
   */
  const shouldUseTextarea = (placeholder: PlaceholderInfo): boolean => {
    const name = placeholder.name.toLowerCase();
    const hasLongContentKeyword =
      name.includes('description') ||
      name.includes('content') ||
      name.includes('text') ||
      name.includes('message') ||
      name.includes('details') ||
      name.includes('notes');

    return hasLongContentKeyword || placeholder.value.length > 50;
  };

  return (
    <div className={`space-y-4 ${className}`}>
      <h3 className="text-lg font-semibold" id="placeholder-form-heading">
        Fill Placeholder Values
      </h3>

      <form
        className="space-y-4"
        aria-labelledby="placeholder-form-heading"
        onSubmit={(e) => e.preventDefault()}
      >
        {placeholders.map((placeholder) => {
          const useTextarea = shouldUseTextarea(placeholder);
          const inputId = `placeholder-${placeholder.name}`;
          const errorId = `error-${placeholder.name}`;
          const hasError = placeholder.touched && placeholder.error;

          return (
            <div key={placeholder.name} className="space-y-2">
              {/* Label with required indicator */}
              <Label htmlFor={inputId}>
                {placeholder.displayName || placeholder.name.replace(/_/g, ' ')}
                {placeholder.isRequired && (
                  <span className="text-destructive ml-1" aria-label="required">
                    *
                  </span>
                )}
              </Label>

              {/* Input or Textarea based on field characteristics */}
              {useTextarea ? (
                <Textarea
                  id={inputId}
                  value={placeholder.value}
                  onChange={(e) => onPlaceholderChange(placeholder.name, e.target.value)}
                  onBlur={() => onPlaceholderBlur?.(placeholder.name)}
                  disabled={disabled}
                  required={placeholder.isRequired}
                  aria-invalid={hasError ? 'true' : 'false'}
                  aria-describedby={hasError ? errorId : undefined}
                  placeholder={`Enter ${placeholder.displayName || placeholder.name.replace(/_/g, ' ')}...`}
                  className="min-h-[100px] resize-y"
                />
              ) : (
                <Input
                  id={inputId}
                  type="text"
                  value={placeholder.value}
                  onChange={(e) => onPlaceholderChange(placeholder.name, e.target.value)}
                  onBlur={() => onPlaceholderBlur?.(placeholder.name)}
                  disabled={disabled}
                  required={placeholder.isRequired}
                  aria-invalid={hasError ? 'true' : 'false'}
                  aria-describedby={hasError ? errorId : undefined}
                  placeholder={`Enter ${placeholder.displayName || placeholder.name.replace(/_/g, ' ')}...`}
                />
              )}

              {/* Validation error display */}
              {hasError && (
                <p
                  id={errorId}
                  className="text-sm text-destructive"
                  role="alert"
                >
                  {placeholder.error}
                </p>
              )}

              {/* Helper text for guidance (only if no error) */}
              {!hasError && placeholder.isRequired && !placeholder.touched && (
                <p className="text-sm text-muted-foreground">
                  Required field
                </p>
              )}
            </div>
          );
        })}
      </form>
    </div>
  );
}
