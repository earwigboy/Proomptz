import { Textarea } from '../ui/textarea';
import { Button } from '../ui/button';

interface TemplateEditorProps {
  /** Current content to edit */
  content: string;

  /** Callback when content changes */
  onContentChange: (content: string) => void;

  /** Whether content has been edited */
  hasEdits: boolean;

  /** Callback to reset content to original */
  onReset: () => void;

  /** Whether editor is disabled */
  disabled?: boolean;

  /** Optional CSS class for container */
  className?: string;
}

export default function TemplateEditor({
  content,
  onContentChange,
  hasEdits,
  onReset,
  disabled = false,
  className = '',
}: TemplateEditorProps) {
  return (
    <div className={`flex flex-col space-y-4 ${className}`}>
      {/* Header with Reset button (T024: only shown when hasEdits is true) */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Edit Template Content</h3>
        {hasEdits && (
          <Button
            variant="outline"
            size="sm"
            onClick={onReset}
            disabled={disabled}
            type="button"
          >
            Reset to Original
          </Button>
        )}
      </div>

      {/* T004-T006: Enhanced textarea with performance optimizations */}
      {/* T004: minHeight 500px for 30+ visible lines */}
      {/* T005: Performance optimizations (spellCheck, autoComplete, etc.) */}
      {/* T006: Monospace font for improved rendering */}
      <Textarea
        value={content}
        onChange={(e) => onContentChange(e.target.value)}
        disabled={disabled}
        spellCheck="false"
        autoComplete="off"
        autoCorrect="off"
        autoCapitalize="off"
        aria-label="Edit template content"
        className="flex-1 min-h-[500px] font-mono text-sm resize-y"
        style={{ resize: 'vertical' }}
        placeholder="Edit your template content here..."
      />

      {hasEdits && (
        <p className="text-sm text-muted-foreground">
          Content has been modified. Click "Reset to Original" to revert changes.
        </p>
      )}
    </div>
  );
}
