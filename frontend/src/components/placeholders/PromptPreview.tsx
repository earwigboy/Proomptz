import { generatePrompt } from '../../lib/utils/placeholders';
import { Badge } from '../ui/badge';

interface PromptPreviewProps {
  templateContent: string;
  placeholderValues: Record<string, string>;
  className?: string;
  hasEdits?: boolean;
}

export default function PromptPreview({
  templateContent,
  placeholderValues,
  className = '',
  hasEdits = false,
}: PromptPreviewProps) {
  const generatedPrompt = generatePrompt(templateContent, placeholderValues);

  return (
    <div className={`prompt-preview flex flex-col ${className}`}>
      <div className="flex items-center justify-between mb-2">
        <h3 id="prompt-preview-heading">Preview</h3>
        {/* T040: Edited badge when hasEdits is true */}
        {hasEdits && (
          <Badge variant="secondary">
            Edited
          </Badge>
        )}
      </div>
      {/* T037: flex-1 for flexible height */}
      {/* T038: Responsive min-height classes */}
      {/* T039: overflow-y-auto for scrolling */}
      <div
        className="flex-1 min-h-[300px] md:min-h-[400px] lg:min-h-[500px] overflow-y-auto"
        style={{
          background: '#1a1a1a',
          border: '1px solid #444',
          borderRadius: '8px',
          padding: '1rem',
          whiteSpace: 'pre-wrap',
          fontFamily: 'monospace',
        }}
        role="region"
        aria-labelledby="prompt-preview-heading"
        aria-live="polite"
        aria-atomic="true"
        tabIndex={0}
      >
        {generatedPrompt}
      </div>
    </div>
  );
}
