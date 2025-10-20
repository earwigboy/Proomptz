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

      {/* T023: Textarea for content editing */}
      {/* T025: Monospace font and responsive min-height */}
      {/* T026: aria-label for accessibility */}
      <Textarea
        value={content}
        onChange={(e) => onContentChange(e.target.value)}
        disabled={disabled}
        aria-label="Edit template content"
        className="flex-1 min-h-[300px] md:min-h-[400px] font-mono text-sm resize-y"
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
